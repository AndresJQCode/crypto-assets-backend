using Api.Application.Commands.SystemConfigurationCommands;
using Api.Application.Dtos.SystemConfiguration;
using Api.Application.Queries.SystemConfigurationQueries;
using Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Apis.SystemConfigurationEndpoints;

public static class SystemConfigurationApi
{
    public static RouteGroupBuilder MapSystemConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/system-config")
            .WithTags("System Configuration");

        group.MapGet("/{key}", GetConfigByKey)
            .RequirePermission("SystemConfiguration", "Read")
            .WithName("GetSystemConfigurationByKey")
            .WithDescription("Get system configuration by key")
            .Produces<SystemConfigurationDto>()
            .Produces(404);

        group.MapPut("/{key}", UpdateConfig)
            .RequirePermission("SystemConfiguration", "Update")
            .WithName("UpdateSystemConfiguration")
            .WithDescription("Update system configuration value")
            .Produces(200)
            .Produces(404);

        group.MapPatch("/{key}/toggle", ToggleConfig)
            .RequirePermission("SystemConfiguration", "Update")
            .WithName("ToggleSystemConfiguration")
            .WithDescription("Enable or disable a system configuration")
            .Produces(200)
            .Produces(404);

        // Convenience endpoints for order processing control
        group.MapPost("/order-processing/enable", EnableOrderProcessing)
            .RequirePermission("SystemConfiguration", "Update")
            .WithName("EnableOrderProcessing")
            .WithDescription("Enable order processing from Pub/Sub (for post-deployment)")
            .Produces(200);

        group.MapPost("/order-processing/disable", DisableOrderProcessing)
            .RequirePermission("SystemConfiguration", "Update")
            .WithName("DisableOrderProcessing")
            .WithDescription("Disable order processing from Pub/Sub (for pre-deployment)")
            .Produces(200);

        return group;
    }

    private static async Task<IResult> GetConfigByKey(
        [FromRoute] string key,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetSystemConfigurationByKeyQuery(key);
        var result = await mediator.Send(query, cancellationToken);

        return result == null
            ? Results.NotFound(new { Message = $"Configuration with key '{key}' not found" })
            : Results.Ok(result);
    }

    private static async Task<IResult> UpdateConfig(
        [FromRoute] string key,
        [FromBody] UpdateSystemConfigurationDto dto,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSystemConfigurationCommand(key, dto.Value, dto.Description);
        await mediator.Send(command, cancellationToken);

        return Results.Ok(new { Message = $"Configuration '{key}' updated successfully" });
    }

    private static async Task<IResult> ToggleConfig(
        [FromRoute] string key,
        [FromBody] ToggleSystemConfigurationRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ToggleSystemConfigurationCommand(key, request.IsActive);
        await mediator.Send(command, cancellationToken);

        var status = request.IsActive ? "enabled" : "disabled";
        return Results.Ok(new { Message = $"Configuration '{key}' {status} successfully" });
    }

    private static async Task<IResult> EnableOrderProcessing(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        const string key = "OrderProcessing.PubSub.Enabled";
        var command = new ToggleSystemConfigurationCommand(key, true);
        await mediator.Send(command, cancellationToken);

        return Results.Ok(new
        {
            Message = "Order processing enabled. Worker will resume pulling from Pub/Sub.",
            Key = key,
            Value = "true"
        });
    }

    private static async Task<IResult> DisableOrderProcessing(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        const string key = "OrderProcessing.PubSub.Enabled";
        var command = new ToggleSystemConfigurationCommand(key, false);
        await mediator.Send(command, cancellationToken);

        return Results.Ok(new
        {
            Message = "Order processing disabled. Worker will stop pulling from Pub/Sub after current batch completes.",
            Key = key,
            Value = "false"
        });
    }
}

public record ToggleSystemConfigurationRequest(bool IsActive);
