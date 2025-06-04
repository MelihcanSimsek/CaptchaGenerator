using CaptchaGenerator.Models.Security;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CaptchaGenerator.Security.Hash;

public sealed class HashHelper : IHashHelper
{
    private readonly HashOptions hashOptions;
    public HashHelper(IOptions<HashOptions> options)
    {
        hashOptions = options.Value;
    }

    public async Task<string> HashText(string text)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(hashOptions.HashSecret + text);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public async Task<bool> ValidateHash(string computedText, string input)
    {
        string computedInput = await HashText(input);
      
        return computedInput == computedText;
    }

   
}
