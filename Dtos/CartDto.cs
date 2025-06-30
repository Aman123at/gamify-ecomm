namespace GamifyApi.Dtos;

public class CartProductRequest
{
    public required string ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class CartRequest
{
    public List<CartProductRequest> Products { get; set; } = new List<CartProductRequest>();
}

public class CartResponse
{
    public required string Id { get; set; }
    public List<CartProductResponse> Products { get; set; } = new List<CartProductResponse>();
}

public class CartProductResponse
{
    public required string ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public ProductResponse Product { get; set; } = new ProductResponse();
}

public class QuantityRequest
{
    public required string CartId { get; set; }
    public required string ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
}