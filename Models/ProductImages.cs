namespace GamifyApi.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductImages
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("Product")]
    public required string ProductId { get; set; }
}