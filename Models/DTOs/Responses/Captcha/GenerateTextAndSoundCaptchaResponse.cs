namespace CaptchaGenerator.Models.DTOs.Responses.Captcha;

public sealed record GenerateTextAndSoundCaptchaResponse(string Token,string CaptchaImage64,string CaptchaSound64,string MimeType,string SoundType);