namespace CaptchaGenerator.Models.DTOs.Requests.Auth;

public sealed record LoginRequestDto(
    string Email,
    string Password,
    string Token,
    string Answer);
