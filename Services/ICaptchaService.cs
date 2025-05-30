using CaptchaGenerator.Models.DTOs.Requests;
using CaptchaGenerator.Models.DTOs.Responses;

namespace CaptchaGenerator.Services;

public interface ICaptchaService
{
    Task<CaptchaCheckResponseDto> CheckCaptcha(CaptchaCheckRequestDto requestDto,string ip);
    Task<GenerateCaptchaResponse> GenerateCaptcha(string ip);
    Task<GenerateSoundCaptchaResponse> GenerateSoundCaptcha(string ip);
    Task<GenerateTextAndSoundCaptchaResponse> GenerateTextAndSoundCaptcha(string ip);
}
