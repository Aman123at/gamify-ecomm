using System.Security.Claims;
using CloudinaryDotNet.Actions;
using GamifyApi.Dtos;
using GamifyApi.Middlwares;
using GamifyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;

namespace GamifyApi.Controllers;

[Route("/api/v1/admin")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class AdminController : ControllerBase
{
    private readonly ILogger<AddressController> _logger;
    private readonly GamifyDbContext _context;

    public AdminController(GamifyDbContext context, ILogger<AddressController> logger)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all registered users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="500">Internal server error while fetching users</response>
    [HttpGet("users")]
    [Authorize]
    [Admin]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.FullName,
                    u.Email,
                    u.ProfilePictureUrl,
                    u.City,
                    u.State,
                    u.Country,
                    u.Role
                })
                .ToListAsync();

            return Ok(new { Users = users });
        }
        catch (System.Exception)
        {
            // Log the exception (not implemented here)
            return StatusCode(500, "An error occurred while fetching users.");
        }
    }


    /// <summary>
    /// Get all orders
    /// </summary>
    /// <returns>List of all orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="500">Internal server error while fetching orders.</response>
    [HttpGet("orders")]
    [Authorize]
    [Admin]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    public async Task<IActionResult> GetAllOrders()
    {
        try
        {
            var orders = await _context.Orders.ToListAsync();

            return Ok(new { Orders = orders });
        }
        catch (System.Exception)
        {
            // Log the exception (not implemented here)
            return StatusCode(500, "An error occurred while fetching orders.");
        }
    }

    /// <summary>
    /// Create a new product category (Admin only)
    /// </summary>
    /// <param name="request">Category information to create</param>
    /// <returns>Created category details</returns>
    /// <response code="200">Category created successfully</response>
    /// <response code="400">Invalid category data provided</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="409">Category with this name already exists</response>
    /// <response code="500">Internal server error while creating category</response>
    [HttpPost("category/create")]
    [Admin]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        if (!request.IsNameValid() || !request.IsDescriptionValid())
            return BadRequest("Invalid category data.");

        try
        {
            // Log the incoming request
            _logger.LogInformation("Received request to create category: {CategoryRequest}", request);

            // check if the category already exists
            var existingCategory = await _context.Categories.AnyAsync(c => c.Name.ToLower() == request.Name.ToLower());
            if (existingCategory)
                return Conflict("Category with this name already exists.");

            // Log the creation of the category
            _logger.LogInformation("{Timestamp} => Creating category with name: {Name}", DateTime.Now.ToString(), request.Name);

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "{Timestamp} => An error occurred while creating category: {CategoryRequest}", DateTime.Now.ToString(), request);

            // Return a generic error response
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all registered users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (Admin role required)</response>
    /// <response code="500">Internal server error while fetching users</response>
    [HttpGet("user/stats")]
    [Authorize]
    [Admin]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(string))]
    public async Task<IActionResult> GetUsersStats([FromQuery] int city = 0,
            [FromQuery] int country = 0,
            [FromQuery] int state = 0,
            [FromQuery] int excludeSeller = 0)
    {
        if (city == 0 && country == 0 && state == 0)
        {
            return BadRequest("Required atleast one option from city,state,country");
        }
        try
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(u => u.Role != "Admin");
            if (excludeSeller == 1) {
                query = query.Where(u => u.Role != "Seller");
            }
            if (city == 1)
            {
                var users = await query
                .Select(u => u.City)
                .Where(c => c != string.Empty)
                .Distinct()
                .ToListAsync();

                return Ok(new { Users = users });
            }
            else if (state == 1)
            {
                var users = await query
                .Select(u => u.State)
                .Where(c => c != string.Empty)
                .Distinct()
                .ToListAsync();

                return Ok(new { Users = users });
            }
            else if (country == 1)
            {
                var users = await query
                .Select(u => u.Country)
                .Where(c => c != string.Empty)
                .Distinct()
                .ToListAsync();

                return Ok(new { Users = users });
            }
            else
            {
                return BadRequest("No query input found");
            }
        }
        catch (System.Exception)
        {
            // Log the exception (not implemented here)
            return StatusCode(500, "An error occurred while fetching user stats.");
        }
    }
}