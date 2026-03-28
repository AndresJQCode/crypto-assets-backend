using Domain.AggregatesModel.TenantAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Commands.TenantCommands;

internal sealed class DeleteTenantCommandHandler(
    ITenantRepository tenantRepository,
    UserManager<User> userManager) : IRequestHandler<DeleteTenantCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        Tenant? tenant = await tenantRepository.GetById(request.Id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Tenant no encontrado.");

        int hasUsers = await userManager.Users.CountAsync(u => u.TenantId == request.Id, cancellationToken);
        if (hasUsers > 0)
        {
            throw new BadRequestException("No se puede eliminar el tenant porque tiene usuarios asociados.");
        }

        _ = tenantRepository.Delete(tenant);
        _ = await tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return Unit.Value;
    }
}
