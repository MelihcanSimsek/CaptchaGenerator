namespace CaptchaGenerator.Models.DTOs.Responses;

public sealed record GenerateSoundCaptchaResponse(string Token,string CaptchaSound64,string SoundType);
