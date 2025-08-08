namespace Server.Services;
/// <summary>
/// DomainValidationRequest is a class that represents a request for domain validation.
/// It includes properties for the URL and email to be validated.
/// </summary>
public sealed class DomainValidationRequest
{
    public string? Url { get; set; }
    public string? Email { get; set; }
}
