namespace CaptchaGenerator.Services;

using Model;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Drawing.Drawing2D;
public class CaptchaService
{
    private readonly Random random;
    private readonly Dictionary<Guid, Captcha> pairs;
    public CaptchaService(Random random)
    {
        this.random = random;
        pairs = new();
    }

    private const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public async Task<string> GenerateCaptchaString(int length = 6)
    {
        StringBuilder captchaBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            captchaBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return captchaBuilder.ToString();
    }

    public async Task<CaptchaResponse> CheckCaptcha(Guid id,string captchaText)
    {
        if(pairs.ContainsKey(id) && pairs[id].CaptchaText == captchaText)
        {
            pairs.Remove(id);
            return new("You are human", true);
        }

        pairs.Remove(id);
        return new("You are failed", false);
    }

    public async Task<Captcha> GenerateCaptcha(string captchaText)
    {
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
        int slideAmount = random.Next(7,13);
        int textSpacing = 30;

        double gaussSigmaValue = 1.5;


        for (int i = 0; i < 8; i++)
        {
            int currentWidth = i * verticalSpacing;
            Color lineColor = Color.FromArgb(255, random.Next(180, 255), random.Next(180, 255), random.Next(180, 255));
            pen.Color = lineColor;
            Point start = new Point(currentWidth+ slideAmount, 0);
            Point end = new Point(currentWidth, height);
            graphics.DrawLine(pen, start, end);
        }

        for (int i = 0; i < 8; i++)
        {
            int currentHeight = i * horizontalSpacing;
            Color lineColor = Color.FromArgb(255, random.Next(180, 255), random.Next(180, 255), random.Next(180, 255));
            pen.Color = lineColor;
            Point start = new Point(0, currentHeight);
            Point end = new Point(width, currentHeight+ slideAmount);
            graphics.DrawLine(pen, start, end);
        }

        for (int i = 0; i < captchaText.Length; i++)
        {
            char charValue = captchaText[i];
            brush.Color = Color.FromArgb(255, random.Next(0, 199), random.Next(0, 199), random.Next(0, 199));

            graphics.ResetTransform();
            graphics.TranslateTransform(slideAmount + (i * textSpacing), height / 2);
            graphics.RotateTransform((float)random.Next(-30, 30));

            graphics.DrawString(charValue.ToString(), font, brush,-5,-5);
        }

      
       

        using var memoryStream = new MemoryStream();
        Bitmap blurredImage = await GaussFilter(bitMap, await CalculateGaussFilter(gaussSigmaValue));
        blurredImage.Save(memoryStream, ImageFormat.Png);
        var base64 = Convert.ToBase64String(memoryStream.ToArray());

        Captcha response = new Captcha(captchaText, base64, "image/png");
        pairs.Add(response.Id, response);

        return response;
    }

    private async Task<int[,]> CalculateGaussFilter(double sigma)
    {
        int number = Convert.ToInt32(Math.Round(sigma * 6));
        int matrisSize = number % 2 == 0 ? number - 1 : number;
        int[,] filter = new int[matrisSize, matrisSize];
        double leftValue = 1 / (Math.Sqrt(2 * Math.PI) * sigma);
        double scale = 0;
        for (int i = -matrisSize / 2; i < matrisSize / 2; i++)
        {
            for (int j = -matrisSize / 2; j < matrisSize / 2; j++)
            {
                double value = leftValue * Math.Exp(-((i * i + j + j) / (2 * sigma * sigma)));
                if (i == -matrisSize / 2 && j == -matrisSize / 2)
                {
                    scale = 1 / value;
                }
                filter[i + matrisSize / 2, j + matrisSize / 2] = Convert.ToInt32(Math.Round(scale * value));
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

