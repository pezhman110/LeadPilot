namespace LeadPilot.Application.Auth;

public sealed record AuthTokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);