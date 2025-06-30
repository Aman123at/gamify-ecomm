using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyApi.Models;

public class OrderProduct
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey("Order")]
    public required string OrderId { get; set; }

    [ForeignKey("Product")]
    public required string ProductId { get; set; }
}