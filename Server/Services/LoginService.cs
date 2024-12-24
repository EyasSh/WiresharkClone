namespace Server.Services;
public class LoginRequest
{
    public required string name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required DateOnly dateOnly { get; set; }

}
