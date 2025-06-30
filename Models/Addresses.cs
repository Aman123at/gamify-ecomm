namespace GamifyApi.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Address
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Area { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("User")]
    public required string UserId { get; set; }
}