using System.Security.Claims;
using Api.Application.Commands.AuthCommands;
using Api.Application.Dtos.Auth;
using Api.Application.Dtos.User;
using Api.Application.Queries.Users;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Apis.AuthEndpoints;

internal static class AuthApi
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder api = app.MapGroup("auth");

        _ = api.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Iniciar sesión")
            .WithDescription("Autentica al usuario y devuelve un token JWT")
            .Produces<LoginResponseDto>(200)
            .Produces<BadRequestException>(400)
            .Produces<ProblemHttpResult>(500);

        // _ = api.MapPost("/register", Register)
        //     .WithName("Register")
        //     .WithSummary("Registrar usuario")
        //     .WithDescription("Registra un nuevo usuario con rol Admin y devuelve un token JWT")
        //     .Produces<LoginResponseDto>()
        //     .Produces<BadRequestException>()
        //     .Produces<ProblemHttpResult>();

        _ = api.MapPost("/exchangeCode", ExchangeCode)
            .WithName("ExchangeCode")
            .WithSummary("Intercambiar código de autenticación")
            .WithDescription("Intercambia un código de autenticación por un token JWT")
            .Produces<ExchangeCodeResponseDto>()
            .Produces(400)
            .Produces(401)
            .Produces<ProblemHttpResult>();

        _ = api.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .WithSummary("Renovar token de acceso")
            .WithDescription("Renueva el token de acceso usando un refresh token válido")
            .Produces<LoginResponseDto>()
            .Produces<BadRequestException>()
            .Produces<ProblemHttpResult>();

        _ = api.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Cerrar sesión")
            .WithDescription("Invalida el token del usuario actual. Requiere autorización (usuario autenticado)")
            .RequireAuthorization()
            .Produces(200)
            .Produces<UnAuthorizedException>();

        _ = api.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Obtener información del usuario actual")
            .WithDescription("Devuelve la información del usuario autenticado. Requiere autorización (usuario autenticado)")
            .RequireAuthorization()
            .Produces<AuthUserDto>()
            .Produces<UnAuthorizedException>()
            .Produces<NotFoundException>()
            .Produces<ProblemHttpResult>();

        _ = api.MapPut("/me", UpdateProfile)
            .WithName("UpdateProfile")
            .WithSummary("Actualizar perfil del usuario actual")
            .WithDescription("Permite al usuario autenticado actualizar su nombre y email. Requiere autorización")
            .RequireAuthorization()
            .Produces<AuthUserDto>()
            .Produces<BadRequestException>()
            .Produces<UnAuthorizedException>()
            .Produces<NotFoundException>()
            .Produces<ProblemHttpResult>();

        _ = api.MapPost("/forgotPassword", ForgotPassword)
            .WithName("ForgotPassword")
            .WithSummary("Olvidé mi contraseña")
            .WithDescription("Envía un correo para restablecer la contraseña")
            .Produces<Ok>()
            .Produces<BadRequestException>()
            .Produces<ProblemHttpResult>();
        return api;
    }

    private static async Task<IResult> Login(LoginCommand command, IMediator mediator)
    {
        LoginResponseDto? result = await mediator.Send(command);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> Register(RegisterCommand command, IMediator mediator)
    {
        LoginResponseDto? result = await mediator.Send(command);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> Logout(IMediator mediator, ClaimsPrincipal user)
    {
        string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }
        await mediator.Send(new LogoutCommand(userId));
        return TypedResults.Ok(new { message = "Sesión cerrada exitosamente" });
    }

    private static async Task<IResult> ExchangeCode(ExchangeCodeRequestDto request, IMediator mediator)
    {
        ExchangeCodeCommand command = new()
        {
            Code = request.Code,
            Provider = request.Provider
        };

        try
        {
            ExchangeCodeResponseDto? result = await mediator.Send(command);
            return TypedResults.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(new { message = ex.Message });
        }
        // UnAuthorizedException (ej. usuario no registrado con AllowPublicUsers=false) la maneja ErrorHandlerMiddleware → 401
    }

    private static async Task<IResult> GetCurrentUser(IMediator mediator)
    {
        try
        {
            GetCurrentUserQuery query = new();
            CurrentUserDto? result = await mediator.Send(query);
            return TypedResults.Ok(result);
        }
        catch (UnAuthorizedException)
        {
            return TypedResults.Unauthorized();
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (Exception)
        {
            return TypedResults.Problem("Error interno del servidor");
        }
    }

    private static async Task<IResult> RefreshToken(RefreshTokenRequestDto request, IMediator mediator)
    {
        try
        {
            RefreshTokenCommand? command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            };

            LoginResponseDto? result = await mediator.Send(command);
            return TypedResults.Ok(result);
        }
        catch (UnAuthorizedException ex)
        {
            return TypedResults.BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return TypedResults.BadRequest(new { message = "Error al renovar el token." });
        }
    }

    private static async Task<IResult> UpdateProfile(UpdateProfileCommand request, IMediator mediator, ClaimsPrincipal user)
    {
        AuthUserDto? result = await mediator.Send(request);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> ForgotPassword(ForgotPasswordCommand request, IMediator mediator)
    {
        object? result = await mediator.Send(request);
        return TypedResults.Ok(result);
    }
}
