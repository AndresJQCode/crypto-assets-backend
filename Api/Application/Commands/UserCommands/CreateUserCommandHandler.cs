using System.Security.Cryptography;
using Api.Application.Dtos.User;
using Api.Infrastructure.Services;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Api.Application.Commands.UserCommands;

internal sealed class CreateUserCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager, ITenantContext tenantContext) : IRequestHandler<CreateUserCommand, UserResponseDto>
{
    public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var currentTenantId = tenantContext.GetCurrentTenantId();
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            throw new BadRequestException("Ya existe un usuario con ese email.");

        // UserName = Email (convención del sistema)
        // Identity normaliza automáticamente UserName y Email
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name,
            TenantId = currentTenantId,
        };

        // Crear el usuario con contraseña generada
        string password = GenerarContrasena();
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Error al crear el usuario: {errors}");
        }

        // Obtener los roles del usuario que vienen por el request y asignarlos al usuario
        List<UserRoleDto> userRoles = [];
        foreach (var roleId in request.Roles)
        {
            Role role = await roleManager.FindByIdAsync(roleId) ?? throw new BadRequestException($"Rol con ID {roleId} no encontrado");
            userRoles.Add(new UserRoleDto
            {
                Id = role.Id.ToString(),
                Name = role.Name!,
            });
            _ = await userManager.AddToRoleAsync(user, role.Name!);
        }


        // Mapear a DTO de respuesta
        var userDto = new UserResponseDto
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Name = user.Name,
            IsActive = user.IsActive,
            Roles = userRoles.ToArray()
        };

        return userDto;
    }

    // Función para generar una contraseña aleatoria con al menos una mayúscula, una minúscula, un número y un carácter especial (criptográficamente segura)
    private static string GenerarContrasena(int longitud = 12)
    {
        const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string minusculas = "abcdefghijklmnopqrstuvwxyz";
        const string numeros = "0123456789";
        const string especiales = "!@#$%^&*()-_=+[]{}|;:,.<>?";
        string caracteres = mayusculas + minusculas + numeros + especiales;

        // Garantizar al menos un carácter de cada tipo usando RNG criptográfico
        var contrasena = new List<char>
        {
            mayusculas[GetSecureRandomIndex(mayusculas.Length)],
            minusculas[GetSecureRandomIndex(minusculas.Length)],
            numeros[GetSecureRandomIndex(numeros.Length)],
            especiales[GetSecureRandomIndex(especiales.Length)]
        };

        for (int i = contrasena.Count; i < longitud; i++)
        {
            contrasena.Add(caracteres[GetSecureRandomIndex(caracteres.Length)]);
        }

        // Mezclar con Fisher-Yates usando RNG criptográfico
        for (int i = contrasena.Count - 1; i > 0; i--)
        {
            int j = GetSecureRandomIndex(i + 1);
            (contrasena[i], contrasena[j]) = (contrasena[j], contrasena[i]);
        }

        return new string(contrasena.ToArray());
    }

    private static int GetSecureRandomIndex(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            return 0;
        }

        const int byteCount = 4;
        byte[] bytes = RandomNumberGenerator.GetBytes(byteCount);
        uint value = BitConverter.ToUInt32(bytes, 0);
        return (int)(value % (uint)maxExclusive);
    }
}
