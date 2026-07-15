namespace LeadPilot.Application.Common;

public sealed class ProblemDetailsException : Exception
{
    public ProblemDetailsException(int statusCode, string title, string detail)
        : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
    }

    public int StatusCode { get; }

    public string Title { get; }

    public string Detail { get; }
}