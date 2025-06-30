using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyApi.Models;

public class CartProduct
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [ForeignKey("Cart")]
    public required string CartId { get; set; }
    
    [ForeignKey("Product")]
    public required string ProductId { get; set; }
    
    public int Quantity { get; set; } = 1;
}