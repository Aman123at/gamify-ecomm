using CloudinaryDotNet;
using GamifyApi.Dtos;

public static class CloudinaryExtensions
{
    public static List<PresignedUrlResponse> GenerateBulkUploadUrls(
        this Cloudinary cloudinary, 
        string productId, 
        int count)
    {
        var responses = new List<PresignedUrlResponse>();
        
        for (int i = 0; i < count; i++)
        {
            var publicId = $"{productId}-{Guid.NewGuid()}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var uploadParams = new SortedDictionary<string, object>
            {
                { "timestamp", timestamp },
                { "public_id", publicId }
            };

            var signature = cloudinary.Api.SignParameters(uploadParams);

            responses.Add(new PresignedUrlResponse
            {
                Signature = signature,
                ApiKey = cloudinary.Api.Account.ApiKey,
                Timestamp = timestamp,
                PublicId = publicId,
                UploadUrl = $"https://api.cloudinary.com/v1_1/{cloudinary.Api.Account.Cloud}/image/upload",
                AccessUrl = $"https://res.cloudinary.com/{Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")}/image/upload/v{timestamp}/{publicId}",
            });
        }
        
        return responses;
    }
}