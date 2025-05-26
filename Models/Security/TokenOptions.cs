namespace CaptchaGenerator.Model.Security;

public class TokenOptions
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public string Secret { get; set; }
    public int TokenExpiredTime { get; set; }
}
