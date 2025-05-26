namespace CaptchaGenerator.Models.DTOs.Responses;

public sealed record CaptchaCheckResponseDto(string Message, bool IsSuccess);