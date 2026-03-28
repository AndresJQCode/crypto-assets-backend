using Api.Application.Dtos.Tenant;
using Domain.AggregatesModel.TenantAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Api.Application.Commands.TenantCommands;

internal sealed class UpdateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IIdentityService identityService) : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        Tenant? tenant = await tenantRepository.GetById(request.Id, tracking: true, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Tenant no encontrado.");
        Guid? modifiedBy = identityService.GetCurrentUserId();
        tenant.Update(request.Name, request.Slug, modifiedBy);
        tenant.SetActive(request.IsActive);
        _ = tenantRepository.Update(tenant);
        _ = await tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return new TenantDto
        {
            Id = tenant.Id.ToString(),
            Name = tenant.Name,
            Slug = tenant.Slug,
            IsActive = tenant.IsActive
        };
    }
}
