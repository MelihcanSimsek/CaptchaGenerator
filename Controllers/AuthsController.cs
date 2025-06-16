using CaptchaGenerator.Models.DTOs.Requests.Auth;
using CaptchaGenerator.Models.DTOs.Responses.Auth;
using CaptchaGenerator.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaptchaGenerator.Controllers;

[Route("api/[controller]/")]
[ApiController]
public class AuthsController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthsController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequestDto registerRequest)
    {
        string ip = GetIpAddress();

        var response = await authService.Register(registerRequest, ip);
        return Ok(response);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequestDto loginRequest)
    {
        string ip = GetIpAddress();
        var response = await authService.Login(loginRequest, ip);
        return Ok(response);
    }

    [HttpGet("LoginWithGoogle")]
    public async Task<IActionResult> LoginWithGoogle()
    {
        GoogleAuthenticationResponse result = await authService.GetGoogleAuthentication();
        return Ok(result);
    }

    [HttpGet("Callback")]
    public async Task<IActionResult> Callback([FromQuery] GoogleCallbackRequest request)
    {
        TokenExchangeResponse tokenExchangeResponse = await authService.ExchangeGoogleTokensWithCode(request);
        if (!tokenExchangeResponse.IsSuccess)
            return BadRequest();

        LoginResponse loginResponse = await authService.LoginWithGoogle(tokenExchangeResponse.GoogleTokens);
        return Ok(loginResponse);
    }

    [HttpGet("TestAuth")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> TestAuth()
    {
        return Ok();
    }


    [HttpGet("TestPermissionAuth")]
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
