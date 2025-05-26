using CaptchaGenerator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaptchaGenerator.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        private readonly CaptchaService captchaService;

        public CaptchaController(CaptchaService captchaService)
        {
            this.captchaService = captchaService;
        }


        [HttpGet]
        public async Task<IActionResult> GetCaptcha()
        {
            var value = await captchaService.GenerateCaptchaString();
            var result = await captchaService.GenerateCaptcha(value);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CheckCaptcha([FromQuery] Guid id,[FromQuery] string captchaText)
        {
            var result = await captchaService.CheckCaptcha(id, captchaText);
            return Ok(result);
        }


    }
}
