using CaptchaGenerator.Models.DTOs.Requests;
using CaptchaGenerator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaptchaGenerator.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaService captchaService;

        public CaptchaController(ICaptchaService captchaService)
        {
            this.captchaService = captchaService;
        }


        [HttpGet]
        public async Task<IActionResult> GetCaptcha()
        {
            string ip =  GetIpAddress();
            var result = await captchaService.GenerateCaptcha(ip);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CheckCaptcha(CaptchaCheckRequestDto request)
        {
            string ip = GetIpAddress();
            var result = await captchaService.CheckCaptcha(request, ip);
            return Ok(result);
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For")) return Request.Headers["X-Forwarded-For"];
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}
