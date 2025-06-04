using System.Security.Cryptography;
using System.Text;

namespace CaptchaGenerator.Security.Password;

public static class PasswordHelper
{
    public static void CreatePassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var sha512 = new HMACSHA512();
        passwordSalt = sha512.Key;
        passwordHash = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public static bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var sha512 = new HMACSHA512(passwordSalt);
        byte[] computedPassword = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
        for (int i = 0; i < passwordHash.Length; i++)
        {
            if (passwordHash[i] != computedPassword[i]) return false;
        }
        return true;
    }
}
