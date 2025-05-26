namespace CaptchaGenerator.Services;

using CaptchaGenerator.Constants;
using CaptchaGenerator.Models.DTOs.Requests;
using CaptchaGenerator.Models.DTOs.Responses;
using CaptchaGenerator.Security.Hash;
using CaptchaGenerator.Security.Token;
using Model;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public sealed class CaptchaService : ICaptchaService
{
    private readonly Random random;
    private readonly ITokenHelper tokenHelper;
    private readonly IHashHelper hashHelper;
    public CaptchaService(Random random, ITokenHelper tokenService, IHashHelper hashService)
    {
        this.random = random;
        this.tokenHelper = tokenService;
        this.hashHelper = hashService;
    }

    public async Task<CaptchaCheckResponseDto> CheckCaptcha(CaptchaCheckRequestDto requestDto, string ip)
    {
        bool isTokenValid = await tokenHelper.IsTokenExpired(requestDto.Token);
        if (isTokenValid) return new(CaptchaConstant.Messages.TokenIsNotValid, false);

        var principal = await tokenHelper.GetPrincipal(requestDto.Token);

        string tokenIp = principal.FindFirstValue(claimType: ClaimTypes.NameIdentifier);
        if (tokenIp != ip) return new(CaptchaConstant.Messages.IpIsNotValid, false);


        string hashedCaptcha = principal.FindFirstValue(claimType: ClaimTypes.Name);
        if (!await hashHelper.ValidateHash(hashedCaptcha, requestDto.Answer))
            return new(CaptchaConstant.Messages.CaptchaIsNotValid, false);

        return new(CaptchaConstant.Messages.CaptchaIsValid, true);
    }

    public async Task<GenerateCaptchaResponse> GenerateCaptcha(string ip)
    {
        string captchaText = await GenerateCaptchaString();

        int width = 200;
        int height = 100;

        using Bitmap bitMap = new Bitmap(width, height);
        using Graphics graphics = Graphics.FromImage(bitMap);

        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.Clear(Color.White);

        using Font font = new Font("Arial", 24);
        using SolidBrush brush = new SolidBrush(Color.Black);
        using var pen = new Pen(Color.Black, 3);

        int verticalSpacing = 25;
        int horizontalSpacing = 10;
        int slideAmount = random.Next(7, 13);
        int textSpacing = 30;
        double gaussSigmaValue = 1.5;

        await DrawVerticalLines(graphics, pen, verticalSpacing, slideAmount, height);
        await DrawHorizontalLines(graphics, pen, horizontalSpacing, slideAmount, width);
        await DrawCharacters(graphics, brush, font, slideAmount, textSpacing, captchaText, height);

        using var memoryStream = new MemoryStream();
        Bitmap blurredImage = await GaussFilter(bitMap, await CalculateGaussFilter(gaussSigmaValue));
        blurredImage.Save(memoryStream, ImageFormat.Png);

        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        string hashedText = await hashHelper.HashText(captchaText);
        var _token = await tokenHelper.CreateToken(hashedText, ip);
        string token = new JwtSecurityTokenHandler().WriteToken(_token);
        string mimeType = "image/png";

        GenerateCaptchaResponse response = new(token, base64, mimeType);
        return response;
    }

    private async Task DrawVerticalLines(Graphics graphics,Pen pen,int verticalSpacing,int slideAmount,int height)
    {

        for (int i = 0; i < 8; i++)
        {
            int currentWidth = i * verticalSpacing;
            Color lineColor = Color.FromArgb(255, random.Next(180, 255), random.Next(180, 255), random.Next(180, 255));
            pen.Color = lineColor;
            Point start = new Point(currentWidth + slideAmount, 0);
            Point end = new Point(currentWidth, height);
            graphics.DrawLine(pen, start, end);
        }
    }

    private async Task DrawHorizontalLines(Graphics graphics,Pen pen,int horizontalSpacing,int slideAmount,int width)
    {
        for (int i = 0; i < 8; i++)
        {
            int currentHeight = i * horizontalSpacing;
            Color lineColor = Color.FromArgb(255, random.Next(180, 255), random.Next(180, 255), random.Next(180, 255));
            pen.Color = lineColor;
            Point start = new Point(0, currentHeight);
            Point end = new Point(width, currentHeight + slideAmount);
            graphics.DrawLine(pen, start, end);
        }
    }

    private async Task DrawCharacters(Graphics graphics,SolidBrush brush,Font font,int slideAmount,int textSpacing,string captchaText,int height)
    {
        for (int i = 0; i < captchaText.Length; i++)
        {
            char charValue = captchaText[i];
            brush.Color = Color.FromArgb(255, random.Next(0, 199), random.Next(0, 199), random.Next(0, 199));

            graphics.ResetTransform();
            graphics.TranslateTransform(slideAmount + (i * textSpacing), height / 2);
            graphics.RotateTransform((float)random.Next(-30, 30));

            graphics.DrawString(charValue.ToString(), font, brush, -5, -5);
        }
    }

    private async Task<string> GenerateCaptchaString(int length = 6)
    {
        StringBuilder captchaBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            captchaBuilder.Append(CaptchaConstant.Texts.chars[random.Next(CaptchaConstant.Texts.chars.Length)]);
        }

        return captchaBuilder.ToString();
    }
    private async Task<int[,]> CalculateGaussFilter(double sigma)
    {
        int number = Convert.ToInt32(Math.Round(sigma * 6));
        int matrixSize = number % 2 == 0 ? number - 1 : number;
        int[,] filter = new int[matrixSize, matrixSize];
        double leftValue = 1 / (Math.Sqrt(2 * Math.PI) * sigma);
        double scale = 0;
        for (int i = -matrixSize / 2; i < matrixSize / 2; i++)
        {
            for (int j = -matrixSize / 2; j < matrixSize / 2; j++)
            {
                double value = leftValue * Math.Exp(-((i * i + j + j) / (2 * sigma * sigma)));
                if (i == -matrixSize / 2 && j == -matrixSize / 2)
                {
                    scale = 1 / value;
                }
                filter[i + matrixSize / 2, j + matrixSize / 2] = Convert.ToInt32(Math.Round(scale * value));
            }
        }

        return filter;
    }
    private async Task<Bitmap> GaussFilter(Bitmap image, int[,] filter)
    {
        int size = filter.GetLength(0);
        int sumFilterValue = 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                sumFilterValue += filter[i, j];
            }
        }

        Bitmap newImage = new Bitmap(image.Width, image.Height);

        for (int x = size / 2; x < image.Width - size / 2; x++)
        {
            for (int y = size / 2; y < image.Height - size / 2; y++)
            {
                int sumRed = 0;
                int sumGreen = 0;
                int sumBlue = 0;
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        Color pixel = image.GetPixel(x + i - size / 2, y + j - size / 2);
                        sumRed += pixel.R * filter[i, j];
                        sumGreen += pixel.G * filter[i, j];
                        sumBlue += pixel.B * filter[i, j];
                    }
                }
                newImage.SetPixel(x, y, Color.FromArgb(sumRed / sumFilterValue, sumGreen / sumFilterValue, sumBlue / sumFilterValue));
            }
        }

        return newImage;
    }
}

