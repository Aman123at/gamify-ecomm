using Microsoft.AspNetCore.Mvc;

[Route("")]
[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet("")]
    public IActionResult HomeRoute()
    {
        return Ok(new
        {
            Message = $"Welcome to Gamify api, Visit swagger at - {Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:3000"}/swagger/index.html"
        });
    }
}
