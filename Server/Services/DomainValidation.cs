namespace Server.Services;

public sealed class DomainValidationRequest
{
    public string? Url { get; set; }
    public string? Email { get; set; }
}
