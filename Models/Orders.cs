using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyApi.Models;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey("User")]
    public required string UserId { get; set; }

    [ForeignKey("Address")]
    public required string AddressId { get; set; }

    public string PaymentProvider { get; set; } = "stripe";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}