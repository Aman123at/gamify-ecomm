using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyApi.Models;

public class Cart
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey("User")]
    public required string UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}