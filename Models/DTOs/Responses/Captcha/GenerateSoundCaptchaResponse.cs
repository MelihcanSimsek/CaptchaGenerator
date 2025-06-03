namespace CaptchaGenerator.Models.DTOs.Responses.Captcha;

public sealed record GenerateSoundCaptchaResponse(string Token,string CaptchaSound64,string SoundType);
