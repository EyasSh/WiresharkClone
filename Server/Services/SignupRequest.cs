namespace Server.Services;
/// <summary>
/// Represents a request to sign up a new user.
/// Contains the user's name, email, password, and date of birth.
/// </summary>
public class SignupRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required DateOnly date { get; set; }
}