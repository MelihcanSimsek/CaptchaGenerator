namespace CaptchaGenerator.Models.DTOs.Responses.Captcha;

public sealed record GenerateCaptchaResponse(string Token,string CaptchaImage64,string MimeType);
