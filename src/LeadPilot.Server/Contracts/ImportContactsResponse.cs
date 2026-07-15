namespace LeadPilot.Server.Contracts;

public sealed record ImportContactsResponse(int RowsRead, int ContactValuesQueued);