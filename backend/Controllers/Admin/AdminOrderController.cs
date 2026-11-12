using Microsoft.AspNetCore.Mvc;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Services.Errors;

namespace backend.Controllers.Admin;

[ApiController]
[Route("admin/orders")]
public class AdminOrderController: ControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("cart/{itemId:int}")]
    public async Task<IActionResult> AddItemToCart([FromRoute] int itemId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null || !int.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var result = await _orderService.AddItemToCart(userId, itemId);
        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpDelete("cart/{itemId:int}")]
    public async Task<IActionResult> RemoveItemFromCart([FromRoute] int itemId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null || !int.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var result = await _orderService.RemoveItemFromCart(userId, itemId);
        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("cart")]
    public async Task<IActionResult> GetCart()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null || !int.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }
        var result = await _orderService.GetCart(userId);
        return result.ToActionResult(this);
    }
}
