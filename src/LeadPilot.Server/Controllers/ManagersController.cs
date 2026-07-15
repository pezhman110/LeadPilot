using LeadPilot.Application.Managers;
using LeadPilot.Domain.Enums;
using LeadPilot.Server.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadPilot.Server.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/managers")]
[Authorize(Roles = nameof(UserRole.Administrator))]
public sealed class ManagersController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ManagerDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ManagerDto>> CreateAsync(
        Guid tenantId,
        [FromBody] CreateManagerRequest request,
        [FromServices] ManagerService managerService,
        CancellationToken cancellationToken)
    {
        ManagerDto manager = await managerService.CreateAsync(tenantId, request.Email, request.Password, cancellationToken).ConfigureAwait(false);
        return Created($"/api/tenants/{tenantId}/managers/{manager.Id}", manager);
    }
}