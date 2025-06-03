namespace CaptchaGenerator.Model.Security;

public class TokenOptions
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public string CaptchaSecret { get; set; }
    public int CaptchaTokenExpiredTime { get; set; }
    public string AccessSecret { get; set; }
    public int AccessTokenValidityInMinutes { get; set; }
}
