namespace CaptchaGenerator.Models.DTOs.Responses;

public sealed record GenerateCaptchaResponse(string Token,string CaptchaImage64,string MimeType);

