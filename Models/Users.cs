namespace GamifyApi.Models;

enum UserRole
{
    User,
    Admin,
    Seller
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; } = "";

    public string? City { get; set; } = "";

    public string? State { get; set; } = "";

    public string? Country { get; set; } = "";

    public string Role { get; set; } = UserRole.User.ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}