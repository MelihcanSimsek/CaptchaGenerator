namespace CaptchaGenerator.Models.DTOs.Responses.Captcha;

public sealed record CaptchaCheckResponseDto(string Message, bool IsSuccess);