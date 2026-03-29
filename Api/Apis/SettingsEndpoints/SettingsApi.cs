using MediatR;

namespace Api.Apis.SettingsEndpoints;

internal static class SettingsApi
{
    public static RouteGroupBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder api = app.MapGroup("settings");

        api.MapGet("/", (IMediator mediator) => mediator.Send(new GetSettingsQuery()))
            .WithName("GetSettings")
            .WithSummary("Obtener configuración")
            .WithDescription("Obtiene la configuración del sistema")
            .Produces<SettingsResponseDto>(200)
            .Produces<BadRequestException>(400)
            .Produces<ProblemHttpResult>(500);

        return api;
    }
}