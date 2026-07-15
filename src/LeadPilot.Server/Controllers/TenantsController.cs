using LeadPilot.Application.Tenants;
using LeadPilot.Domain.Enums;
using LeadPilot.Server.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadPilot.Server.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Roles = nameof(UserRole.Administrator))]
public sealed class TenantsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TenantDto>> CreateAsync(
        [FromBody] CreateTenantRequest request,
        [FromServices] TenantService tenantService,
        CancellationToken cancellationToken)
    {
        TenantDto tenant = await tenantService.CreateAsync(request.Name, cancellationToken).ConfigureAwait(false);
        return Created($"/api/tenants/{tenant.Id}", tenant);
    }
}