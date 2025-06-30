
namespace GamifyApi.Dtos;
public class OrderRequest
{
    public string AddressId { get; set; } = string.Empty;

    public string PaymentProvider { get; set; } = string.Empty;

    public string Products { get; set; } = string.Empty;
}