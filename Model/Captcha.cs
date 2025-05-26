using System.Buffers.Text;

namespace CaptchaGenerator.Model;

public sealed class Captcha
{
    public Guid Id { get; set; } = Guid.NewGuid(); 
    public string CaptchaText { get; set; }
    public string CaptchaImage64 { get; set; }
    public string MimeType { get; set; }

    public Captcha()
    {
        
    }

    public Captcha(string captchaString,string captchaImage64,string mimeType)
    {
        CaptchaText = captchaString;
        CaptchaImage64 = captchaImage64;
        MimeType = mimeType;
    }
}
