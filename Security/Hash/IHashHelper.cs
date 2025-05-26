namespace CaptchaGenerator.Security.Hash;

public interface IHashHelper
{
    Task<string> HashText(string text);
    Task<bool> ValidateHash(string hashedText, string input);
}
