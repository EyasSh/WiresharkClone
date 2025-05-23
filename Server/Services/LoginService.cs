namespace Server.Services;
/// <summary>
/// Represents a request to log in a user.
/// Contains the user's email and password.
/// </summary>
public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }

}
