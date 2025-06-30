public class AddressRequest
{
    public string Area { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public bool isCountryValid()
    {
        return Country.Length > 0;
    }
    public bool isCityValid()
    {
        return City.Length > 0;
    }
    public bool isStateValid()
    {
        return State.Length > 0;
    }
}