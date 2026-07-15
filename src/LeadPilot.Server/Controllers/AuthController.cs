using LeadPilot.Application.Auth;
using LeadPilot.Server.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LeadPilot.Server.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("token")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthTokenResponse>> LoginAsync(
        [FromBody] AuthRequest request,
        [FromServices] AuthService authService,
        CancellationToken cancellationToken)
    {
        AuthTokenResponse response = await authService.LoginAsync(request.Email, request.Password, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}