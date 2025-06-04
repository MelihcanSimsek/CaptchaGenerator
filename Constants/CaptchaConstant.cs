namespace CaptchaGenerator.Constants;

public sealed class CaptchaConstant
{
    public struct Texts
    {
        public const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    }
    
    public struct Messages
    {
        public const string TokenIsNotValid = "Token is not valid";
        public const string IpIsNotValid = "Ip is not valid";
        public const string CaptchaIsNotValid = "Captcha is not valid";
        public const string CaptchaIsValid = "Captcha is valid";
    }
}
