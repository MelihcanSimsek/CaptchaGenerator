namespace CaptchaGenerator.Models.DTOs.Responses.Auth;

public sealed record TokenExchangeResponse(bool IsSuccess,GoogleTokenResponse? GoogleTokens);
