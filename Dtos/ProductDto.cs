using System.ComponentModel.DataAnnotations;

namespace GamifyApi.Dtos;
public class UploadRequest
{
    public string ImageName { get; set; } = string.Empty;
    public string Folder { get; set; } = "gamify";
}


public class UploadResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string SignedUrl { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class ProductRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Stock { get; set; }
    public string CategoryId { get; set; } = string.Empty;

    public bool IsTitleValid()
    {
        return !string.IsNullOrEmpty(Title) && Title.Length >= 3;
    }

    public bool IsDescriptionValid()
    {
        return !string.IsNullOrEmpty(Description) && Description.Length >= 10;
    }

    public bool IsPriceValid()
    {
        return Price > 0;
    }

    public bool IsStockValid()
    {
        return Stock >= 0;
    }

    public bool IsCategoryIdValid()
    {
        return !string.IsNullOrEmpty(CategoryId);
    }
}

public class ProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Price { get; set; }
    public int Stock { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class PaginatedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public List<T> Data { get; set; } = new List<T>();
}

public class GeneratePresignedUrlRequest
{
    
    [Range(1, 5)]
    public int ImageCount { get; set; } = 1;
}



public class SignatureResponse
{
    public string Signature { get; set; }  = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public string CloudName { get; set; } = string.Empty;
    public string UploadPreset { get; set; } = string.Empty;
}


public class PresignedUrlResponse : SignatureResponse
{
    public string PublicId { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
    public string AccessUrl { get; set; } = string.Empty;
}
