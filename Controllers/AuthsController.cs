using CaptchaGenerator.Models.DTOs.Requests.Auth;
using CaptchaGenerator.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaptchaGenerator.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthsController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthsController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequestDto registerRequest)
    {
        string ip = GetIpAddress();

        var response = await authService.Register(registerRequest, ip);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto loginRequest)
    {
        string ip = GetIpAddress();
        var response = await authService.Login(loginRequest, ip);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> TestAuth()
    {
        return Ok();
    }

    [HttpGet]
    [Authorize(Roles = "moderator,user")]
    public async Task<IActionResult> TestPermissionAuth()
    {
        return Ok();
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For")) return Request.Headers["X-Forwarded-For"];
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
