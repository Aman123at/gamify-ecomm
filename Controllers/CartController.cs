using System.Security.Claims;
using System.Transactions;
using GamifyApi.Dtos;
using GamifyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamifyApi.Controllers;

[ApiController]
[Route("api/v1/cart")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class CartController : ControllerBase
{
    private readonly GamifyDbContext _context;

    public readonly ILogger<CartController> _logger;

    public CartController(GamifyDbContext context, ILogger<CartController> logger)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Get all items in the current user's cart
    /// </summary>
    /// <returns>Cart details with all products and quantities</returns>
    /// <response code="200">Cart items retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Cart not found for user</response>
    /// <response code="500">Internal server error while fetching cart</response>
    [HttpGet("getItems")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CartResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public async Task<IActionResult> GetMyCart()
    {
        var userMail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userMail))
            return Unauthorized("User not authenticated.");

        try
        {
            var user = await _context.Users.FirstAsync(u => u.Email == userMail);
            if (string.IsNullOrEmpty(user.Id))
                return Unauthorized("User not authenticated.");

            var cart = await _context.Carts.FirstAsync(c => c.UserId == user.Id);
            if (cart == null)
                return NotFound("Cart not found.");

            var cartProducts = await (
                from cp in _context.CartProducts
                join p in _context.Products on cp.ProductId equals p.Id
                where cp.CartId == cart.Id
                select new CartProductResponse
                {
                    ProductId = cp.ProductId,
                    Quantity = cp.Quantity,
                    Product = new ProductResponse
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.Stock,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        OwnerId = p.OwnerId,
                        CategoryId = p.CategoryId
                    }
                }).ToListAsync();

            return Ok(new CartResponse
            {
                Id = cart.Id,
                Products = cartProducts
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception caught: {ex.Message}");
            // Log the exception (not implemented here)
            _logger.LogError(ex, "{Timestamp} => An error occurred while fetching products from cart for user.", DateTime.Now.ToString());
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Add a product to the current user's cart
    /// </summary>
    /// <param name="request">Product and quantity to add to cart</param>
    /// <returns>Success message when product is added to cart</returns>
    /// <response code="200">Product added to cart successfully</response>
    /// <response code="400">Insufficient stock available</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Product not found</response>
    /// <response code="500">Internal server error while adding to cart</response>
    [HttpPost("add")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public async Task<IActionResult> AddToCart([FromBody] CartProductRequest request)
    {
        var userMail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userMail))
            return Unauthorized("User not authenticated.");

        var strategy = _context.Database.CreateExecutionStrategy();
        ObjectResult result = StatusCode(500, "Something went wrong");
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FirstAsync(u => u.Email == userMail);
                if (string.IsNullOrEmpty(user.Id))
                {
                    result = Unauthorized("User not authenticated.");
                    return;
                }

                var cart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (cart == null)
                {
                    cart = new Cart { UserId = user.Id };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync(); // Save to get cart ID
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId);

                if (product == null)
                {
                    result = NotFound("Product not found");
                    return;
                }

                if (product.Stock < request.Quantity)
                {
                    result = BadRequest($"Insufficient stock. Available: {product.Stock}");
                    return;
                }

                var existingCartProduct = await _context.CartProducts
                    .FirstOrDefaultAsync(cp =>
                        cp.CartId == cart.Id &&
                        cp.ProductId == request.ProductId);

                if (existingCartProduct != null)
                {
                    // Update quantity if product already in cart
                    existingCartProduct.Quantity += request.Quantity;
                }
                else
                {
                    // Add new cart product
                    var cartProduct = new CartProduct
                    {
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity
                    };
                    _context.CartProducts.Add(cartProduct);
                }

                product.Stock -= request.Quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result = Ok(new { Message = "Product added to cart successfully" });
            }
            catch (TransactionAbortedException txEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(txEx, "Error adding product to cart for user");
                result = StatusCode(500, "An error occurred while adding product to cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong while adding product to cart.");
                result =  StatusCode(500, "Something went wrong while adding product to cart.");
            }
        });
        return result;
    }

    /// <summary>
    /// Change quantity of a product in cart or remove it
    /// </summary>
    /// <param name="request">Cart, product, and operation type (inc/dec/rem)</param>
    /// <returns>Updated cart product information</returns>
    /// <response code="200">Quantity updated successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Cart or product not found</response>
    /// <response code="500">Internal server error while updating quantity</response>
    [HttpPost("quantity")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CartProduct))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public async Task<IActionResult> ChangeQuantity([FromBody] QuantityRequest request)
    {
        var userMail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userMail))
            return Unauthorized("User not authenticated.");

        try
        {
            var user = await _context.Users.FirstAsync(u => u.Email == userMail);
            if (string.IsNullOrEmpty(user.Id))
                return Unauthorized("User not authenticated.");

            var cart = await _context.Carts
                .FindAsync(request.CartId);

            if (cart == null)
            {
                return NotFound("Cart does not exist.");
            }

            var cartProduct = await _context.CartProducts.FirstAsync(cp => cp.ProductId == request.ProductId);
            if (cartProduct == null)
            {
                return NotFound("Product does not exist.");
            }

            int quantity = cartProduct.Quantity;

            quantity = request.Type switch
            {
                "inc" => quantity + 1,
                "dec" => quantity - 1,
                "rem" => 0,
                _ => quantity
            };

            if (quantity == cartProduct.Quantity)
            {
                return Ok(cartProduct);
            }

            if (quantity == 0)
            {
                _context.CartProducts.Remove(cartProduct);
            }
            else
            {
                _context.CartProducts.Update(new CartProduct
                {
                    Id = cartProduct.Id,
                    CartId = cartProduct.Id,
                    ProductId = cartProduct.ProductId,
                    Quantity = quantity
                });
            }
            await _context.SaveChangesAsync();

            return Ok(cartProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while changing/removing cart.");
            return StatusCode(500, "Something went wrong while changing/removing cart.");
        }
    }
}