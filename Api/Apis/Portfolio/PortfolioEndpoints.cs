using Api.Application.Commands.Portfolio;
using Api.Application.Dtos.Portfolio;
using Api.Application.Queries.Portfolio;
using Api.Extensions;
using MediatR;

namespace Api.Apis.Portfolio;

public static class PortfolioEndpoints
{
    public static RouteGroupBuilder MapPortfolioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/portfolio")
            .WithTags("Portfolio");

        // GET /api/portfolio - Get current user's portfolio
        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var query = new GetUserPortfolioQuery();
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .RequirePermission("Portfolio", "Read")
        .WithName("GetUserPortfolio")
        .WithDescription("Get the current user's portfolio")
        .Produces<PortfolioDto>(200)
        .Produces(404);

        // GET /api/portfolio/transactions - Get portfolio transaction history
        group.MapGet("/transactions", async (IMediator mediator, CancellationToken ct) =>
        {
            var query = new GetPortfolioTransactionsQuery();
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .RequirePermission("Portfolio", "Read")
        .WithName("GetPortfolioTransactions")
        .WithDescription("Get portfolio transaction history")
        .Produces<List<PortfolioTransactionDto>>(200)
        .Produces(404);

        // POST /api/portfolio - Create new portfolio
        group.MapPost("/", async (CreatePortfolioCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/portfolio", result);
        })
        .RequirePermission("Portfolio", "Create")
        .WithName("CreatePortfolio")
        .WithDescription("Create a new portfolio with initial capital")
        .Produces<PortfolioDto>(201)
        .Produces(400);

        // POST /api/portfolio/deposit - Add deposit
        group.MapPost("/deposit", async (AddDepositCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .RequirePermission("Portfolio", "Update")
        .WithName("AddDeposit")
        .WithDescription("Add a deposit to the portfolio")
        .Produces<PortfolioDto>(200)
        .Produces(400)
        .Produces(404);

        // POST /api/portfolio/withdrawal - Add withdrawal
        group.MapPost("/withdrawal", async (AddWithdrawalCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .RequirePermission("Portfolio", "Update")
        .WithName("AddWithdrawal")
        .WithDescription("Add a withdrawal from the portfolio")
        .Produces<PortfolioDto>(200)
        .Produces(400)
        .Produces(404);

        // PUT /api/portfolio/initial-capital - Update initial capital
        group.MapPut("/initial-capital", async (UpdateInitialCapitalCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .RequirePermission("Portfolio", "Update")
        .WithName("UpdateInitialCapital")
        .WithDescription("Update initial capital (only allowed before transactions)")
        .Produces<PortfolioDto>(200)
        .Produces(400)
        .Produces(404);

        return group;
    }
}
