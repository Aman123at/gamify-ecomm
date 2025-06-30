namespace GamifyApi.Dtos;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; } = "";
    public string? City { get; set; } = "";
    public string? State { get; set; } = "";
    public string? Country { get; set; } = "";

    public bool IsEmailValid()
    {
        return !string.IsNullOrEmpty(Email) && Email.Contains("@") && Email.Contains(".");
    }

    public bool IsPasswordValid()
    {
        return !string.IsNullOrEmpty(Password) && Password.Length >= 6;
    }

    public bool IsFullNameValid()
    {
        return !string.IsNullOrEmpty(FullName) && FullName.Length >= 3;
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsEmailValid()
    {
        return !string.IsNullOrEmpty(Email) && Email.Contains("@") && Email.Contains(".");
    }
    public bool IsPasswordValid()
    {
        return !string.IsNullOrEmpty(Password) && Password.Length >= 6;
    }
}