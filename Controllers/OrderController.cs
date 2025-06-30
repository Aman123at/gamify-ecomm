using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GamifyApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using RabbitMQ.Client;

namespace GamifyApi.Controllers;

[ApiController]
[Route("/api/v1/order")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class OrderController : ControllerBase
{
    private readonly GamifyDbContext _context;
    private readonly ILogger<OrderController> _logger;

    public OrderController(GamifyDbContext context, ILogger<OrderController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order for the authenticated user
    /// </summary>
    /// <param name="request">Order details including address, payment provider, and products</param>
    /// <returns>Success message when order is created</returns>
    /// <response code="200">Order created successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error while creating order</response>
    [HttpPost("create")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
    {
        var userMail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userMail))
            return Unauthorized("User not authenticated.");

        try
        {
            var user = await _context.Users.FirstAsync(u => u.Email == userMail);
            if (string.IsNullOrEmpty(user.Id))
            {
                return Unauthorized("User not authenticated.");
            }

            var factory = new ConnectionFactory { HostName = "localhost", Port=5672 };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "order", durable: false, exclusive: false, autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "order", body: body);
            Console.WriteLine($" [x] Message Sent.");
            return Ok("Order Created Successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Timestamp} => An error occurred while creating order.", DateTime.Now.ToString());
            return StatusCode(500, "An error occurred while creating order.");
        }
    }

}