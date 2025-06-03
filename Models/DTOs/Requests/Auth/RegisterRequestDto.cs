namespace CaptchaGenerator.Models.DTOs.Requests.Auth;

public sealed record RegisterRequestDto(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    string Token,
    string Answer);
