using CsvHelper;
using LeadPilot.Application.Contacts;
using LeadPilot.Domain.Enums;
using LeadPilot.Server.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace LeadPilot.Server.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/contacts")]
[Authorize]
public sealed class ContactsController : ControllerBase
{
    [HttpPost("import")]
    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Manager))]
    [ProducesResponseType(typeof(ImportContactsResponse), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<ImportContactsResponse>> ImportAsync(
        Guid projectId,
        IFormFile file,
        [FromServices] ContactImportService contactImportService,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new ProblemDetails { Title = "Empty file", Detail = "The uploaded CSV file is empty.", Status = StatusCodes.Status400BadRequest });
        }

        ContactImportSession session = await contactImportService.BeginAsync(projectId, cancellationToken).ConfigureAwait(false);
        int rowsRead = 0;
        int valuesQueued = 0;

        await using Stream stream = file.OpenReadStream();
        using StreamReader reader = new(stream);
        using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

        await csv.ReadAsync().ConfigureAwait(false);
        csv.ReadHeader();
        while (await csv.ReadAsync().ConfigureAwait(false))
        {
            ContactImportCandidate candidate = new(
                csv.TryGetField("email", out string? email) ? email : null,
                csv.TryGetField("phone", out string? phone) ? phone : null);

            rowsRead++;
            valuesQueued += await contactImportService.QueueAsync(session, candidate, cancellationToken).ConfigureAwait(false);
        }

        return Accepted(new ImportContactsResponse(rowsRead, valuesQueued));
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Administrator) + "," + nameof(UserRole.Manager))]
    [ProducesResponseType(typeof(IReadOnlyList<ContactListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContactListItemDto>>> ListAsync(
        Guid projectId,
        [FromServices] ContactQueryService contactQueryService,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ContactListItemDto> items = await contactQueryService.ListAsync(projectId, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpPost("{contactId:guid}/reveal")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    [ProducesResponseType(typeof(RevealedContactDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RevealedContactDto>> RevealAsync(
        Guid projectId,
        Guid contactId,
        [FromBody] RevealContactRequest request,
        [FromServices] ContactRevealService contactRevealService,
        CancellationToken cancellationToken)
    {
        RevealedContactDto revealed = await contactRevealService.RevealAsync(projectId, contactId, request.Reason, cancellationToken).ConfigureAwait(false);
        return Ok(revealed);
    }
}