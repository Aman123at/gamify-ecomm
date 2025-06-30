using GamifyApi.Dtos;
using GamifyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace GamifyApi.Controllers;

[ApiController]
[Route("api/v1/category")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class CategoryController : ControllerBase
{
    private readonly ILogger<CategoryController> _logger;
    private readonly GamifyDbContext _context;
    public CategoryController(GamifyDbContext context, ILogger<CategoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all available product categories
    /// </summary>
    /// <returns>List of all categories with their details</returns>
    /// <response code="200">Categories retrieved successfully</response>
    /// <response code="500">Internal server error while fetching categories</response>
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    public async Task<IActionResult> GetAllCategories()
    {
        try
        {
            var categories = await _context.Categories
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { Categories = categories });
        }
        catch (Exception ex)
        {
            // Log the exception (not implemented here)
            _logger.LogError(ex,"{Timestamp} => An error occurred while fetching categories.", DateTime.Now.ToString());
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}