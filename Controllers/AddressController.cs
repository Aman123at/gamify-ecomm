using System.Security.Claims;
using GamifyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamifyApi.Controllers;

[Route("/api/v1/address")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class AddressController : ControllerBase
{
    private readonly ILogger<AddressController> _logger;
    private readonly GamifyDbContext _context;

    public AddressController(GamifyDbContext context, ILogger<AddressController> logger)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Add a new address for the authenticated user
    /// </summary>
    /// <param name="request">Address information to add</param>
    /// <returns>Success message when address is saved</returns>
    /// <response code="200">Address saved successfully</response>
    /// <response code="400">Invalid address data or user not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error while creating address</response>
    [HttpPost("add")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public async Task<IActionResult> AddNewAddress([FromBody] AddressRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (userEmail == null)
            return BadRequest("User not found.");
        if (!request.isCityValid())
        {
            return BadRequest("City is required.");
        }
        if (!request.isStateValid())
        {
            return BadRequest("State is required.");
        }
        if (!request.isCountryValid())
        {
            return BadRequest("Country is required.");
        }
        try
        {
            var user = await _context.Users.FirstAsync(u => u.Email == userEmail);
            if (user == null)
                return BadRequest("User not found");

            var address = new Address
            {
                City = request.City,
                State = request.State,
                Country = request.Country,
                Area = request.Area,
                PostalCode = request.PostalCode,
                UserId = user.Id
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return Ok("New address saved");
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Something went wrong while creating new address");
            return StatusCode(500, "Something went wrong while creating new address");
        }
    }



    /// <summary>
    /// Get all address for the authenticated user
    /// </summary>
    /// <returns>Success message when address is fetched</returns>
    /// <response code="200">Address fetched successfully</response>
    /// <response code="400">Invalid address data or user not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error while fetching address</response>
    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public async Task<IActionResult> GetUsersAddress()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (userEmail == null)
            return BadRequest("User not found.");
        try
        {
            var user = await _context.Users.FirstAsync(u => u.Email == userEmail);
            if (user == null)
                return BadRequest("User not found");

            var addresses = await _context.Addresses.AnyAsync(u => u.UserId == user.Id);

            return Ok(new
            {
                Message = "Addresses fetched successfully.",
                Data = addresses
            });
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Something went wrong while fetching addresses");
            return StatusCode(500, "Something went wrong while fetching new addresses");
        }
    }

    /// <summary>
    /// Delete address for the authenticated user
    /// </summary>
    /// <returns>Success message when address is deleted</returns>
    /// <response code="200">Address deleted successfully</response>
    /// <response code="400">Invalid address data or user not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error while deleting address</response>
    [HttpDelete("{addressId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public async Task<IActionResult> DeleteAddress([FromRoute] string addressId)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (userEmail == null)
            return BadRequest("User not found.");
        try
        {
            var user = await _context.Users.FirstAsync(u => u.Email == userEmail);
            if (user == null)
                return BadRequest("User not found");

            var address = await _context.Addresses.FirstAsync(a => a.Id == addressId);
            if (address == null)
                return BadRequest("Address not found");

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Addresses deleted successfully.",
            });
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Something went wrong while deleting address.");
            return StatusCode(500, "Something went wrong while deleting address.");
        }
    }
    

    /// <summary>
    /// Delete address for the authenticated user
    /// </summary>
    /// <returns>Success message when address is deleted</returns>
    /// <response code="200">Address deleted successfully</response>
    /// <response code="400">Invalid address data or user not found</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error while deleting address</response>
    [HttpPut("{addressId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public async Task<IActionResult> UpdateAddress([FromRoute] string addressId, [FromBody] AddressRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (userEmail == null)
            return BadRequest("User not found.");
        try
        {
            var user = await _context.Users.FirstAsync(u => u.Email == userEmail);
            if (user == null)
                return BadRequest("User not found");

            var address = await _context.Addresses.FirstAsync(a => a.Id == addressId);
            if (address == null)
                return BadRequest("Address not found");

            

            _context.Addresses.Update(new Address
            {
                Id = address.Id,
                UserId = address.UserId,
                City = request.City != string.Empty ? request.City : address.City,
                State = request.State != string.Empty ? request.State : address.State,
                Country = request.Country != string.Empty ? request.Country : address.Country,
                PostalCode = request.PostalCode != string.Empty ? request.PostalCode : address.PostalCode,
                Area = request.Area != string.Empty ? request.Area : address.Area,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Addresses updated successfully.",
            });
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Something went wrong while updating address.");
            return StatusCode(500, "Something went wrong while updating address.");
        }
    }
}