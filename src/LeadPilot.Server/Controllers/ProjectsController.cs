using LeadPilot.Application.Projects;
using LeadPilot.Domain.Enums;
using LeadPilot.Server.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadPilot.Server.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/projects")]
[Authorize(Roles = nameof(UserRole.Administrator))]
public sealed class ProjectsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProjectDto>> CreateAsync(
        Guid tenantId,
        [FromBody] CreateProjectRequest request,
        [FromServices] ProjectService projectService,
        CancellationToken cancellationToken)
    {
        ProjectDto project = await projectService.CreateAsync(tenantId, request.Name, cancellationToken).ConfigureAwait(false);
        return Created($"/api/tenants/{tenantId}/projects/{project.Id}", project);
    }
}