
namespace GamifyApi.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    [ForeignKey("User")]
    public required string OwnerId { get; set; }

    [ForeignKey("Category")]
    public required string CategoryId { get; set; }

}