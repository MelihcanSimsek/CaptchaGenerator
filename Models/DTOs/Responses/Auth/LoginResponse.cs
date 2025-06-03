namespace CaptchaGenerator.Models.DTOs.Responses.Auth;

public sealed record LoginResponse(bool IsSuccess,string Message,string? AccessToken,string? RefreshToken,DateTime? Expiration);
     