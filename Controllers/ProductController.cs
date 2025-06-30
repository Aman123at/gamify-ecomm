namespace GamifyApi.Controllers;

using System.Security.Claims;
using CloudinaryDotNet;
using GamifyApi.Dtos;
using GamifyApi.Middlwares;
using GamifyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/v1/product")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly GamifyDbContext _context;
    private readonly Cloudinary _cloudinary;


    public ProductController(Cloudinary cloudinary, GamifyDbContext context, ILogger<ProductController> logger)
    {
        _context = context;
        _logger = logger;
        _cloudinary = cloudinary;
    }

    /// <summary>
    /// Get all products with pagination, filtering, and sorting options
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="categoryId">Filter by category ID (optional)</param>
    /// <param name="sortBy">Sort field: price, title, or createdAt (default: createdAt)</param>
    /// <param name="sortOrder">Sort order: asc or desc (default: desc)</param>
    /// <returns>Paginated list of products with metadata</returns>
    /// <response code="200">Products retrieved successfully</response>
    /// <response code="400">Invalid pagination parameters</response>
    /// <response code="500">Internal server error while fetching products</response>
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<ProductResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    public async Task<ActionResult<PaginatedResponse<ProductResponse>>> GetAllProducts([FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? categoryId = null,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] string? sortOrder = "desc")
    {
        // Validate inputs
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("Invalid pagination parameters");

        if (pageSize > 100)
            return BadRequest("Page size cannot exceed 100");

        var query = _context.Products.AsQueryable();

        // filters
        if (!string.IsNullOrEmpty(categoryId))
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        // sorting
        query = sortBy?.ToLower() switch
        {
            "price" => sortOrder == "desc"
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "title" => sortOrder == "desc"
                ? query.OrderByDescending(p => p.Title)
                : query.OrderBy(p => p.Title),
            _ => sortOrder == "desc"
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CreatedAt = p.CreatedAt,
                    CategoryId = p.CategoryId,
                    OwnerId = p.OwnerId,
                    Images = _context.ProductImages
                        .Where(img => img.ProductId == p.Id)
                        .Select(img => img.ImageUrl)
                        .ToList()
                })
                .ToListAsync();

        return Ok(new PaginatedResponse<ProductResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Data = products
        });
    }

    /// <summary>
    /// Create a new product (Seller only)
    /// </summary>
    /// <param name="request">Product information to create</param>
    /// <returns>Created product details</returns>
    /// <response code="200">Product created successfully</response>
    /// <response code="400">Invalid product data or category does not exist</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Seller role required)</response>
    /// <response code="500">Internal server error while creating product</response>
    [HttpPost("create")]
    [Authorize]
    [Seller]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequest request)
    {
        // Validate the request
        if (!request.IsTitleValid() || !request.IsDescriptionValid() || !request.IsPriceValid() || !request.IsStockValid() || !request.IsCategoryIdValid())
        {
            return BadRequest("Invalid product data. Title, Description, Price, Stock, and CategoryId are required.");
        }

        _logger.LogInformation("Creating product with title: {Title}", request.Title);

        try
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            var ownerMail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(ownerMail))
            {
                return Unauthorized("User not authenticated.");
            }

            var ownerId = await _context.Users
                .Where(u => u.Email == ownerMail)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (ownerId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var product = new Product
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the product.");
            return StatusCode(500, "An error occurred while creating the product.");
        }
    }

    /// <summary>
    /// Generate presigned URLs for uploading product images
    /// </summary>
    /// <param name="productId">ID of the product to upload images for</param>
    /// <param name="request">Number of images to generate URLs for (max: 5)</param>
    /// <returns>List of presigned URLs for image upload</returns>
    /// <response code="200">Presigned URLs generated successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error while generating URLs</response>
    [HttpPost("{productId}/generate-presigned-url")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PresignedUrlResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public async Task<IActionResult> GeneratePresignedUrls(
        [FromRoute] string productId,
        [FromBody] GeneratePresignedUrlRequest request)
    {
        // Validate product exists
        var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            return NotFound("Product not found");
        }

        // Generate presigned URLs
        var presignedUrls = _cloudinary.GenerateBulkUploadUrls(
            productId,
            Math.Min(request.ImageCount, 5)); // Max 5 images

        // save image metadata to database
        var imageRecords = presignedUrls.Select(url => new ProductImages
        {
            ProductId = productId,
            ImageUrl = $"https://res.cloudinary.com/{Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")}/image/upload/v{url.Timestamp}/{url.PublicId}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await _context.ProductImages.AddRangeAsync(imageRecords);
        await _context.SaveChangesAsync();

        return Ok(presignedUrls);
    }
}