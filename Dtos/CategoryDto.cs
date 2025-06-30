namespace GamifyApi.Dtos;

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public bool IsNameValid()
    {
        return !string.IsNullOrEmpty(Name) && Name.Length >= 3;
    }

    public bool IsDescriptionValid()
    {
        return !string.IsNullOrEmpty(Description) && Description.Length >= 10;
    }
}
public class CategoryResponse
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}