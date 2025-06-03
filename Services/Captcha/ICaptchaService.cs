using CaptchaGenerator.Models.DTOs.Requests.Captcha;
using CaptchaGenerator.Models.DTOs.Responses.Captcha;

namespace CaptchaGenerator.Services.Captcha;

public interface ICaptchaService
{
    Task<CaptchaCheckResponseDto> CheckCaptcha(CaptchaCheckRequestDto requestDto,string ip);
    Task<GenerateCaptchaResponse> GenerateCaptcha(string ip);
    Task<GenerateSoundCaptchaResponse> GenerateSoundCaptcha(string ip);
    Task<GenerateTextAndSoundCaptchaResponse> GenerateTextAndSoundCaptcha(string ip);
}
