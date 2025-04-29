using SkiaSharp;

public static class InsertImageQRCode
{
    public static byte[] AddLogoToQrCode(byte[] qrBytes, byte[] logoBytes, int logoWidthPercent, int logoHeightPercent)
    {
        using var qrBitmap = SKBitmap.Decode(qrBytes);
        if (qrBitmap == null)
        {
            throw new ArgumentException("❗ Невірний формат QR-коду.");
        }
        
        using var logoBitmap = SKBitmap.Decode(logoBytes);
        if (logoBitmap == null)
        {
            throw new ArgumentException("❗ Невірний формат логотипа.");
        }
        
        if (logoBitmap.ColorType != SKColorType.Rgba8888 && logoBitmap.ColorType != SKColorType.Bgra8888)
        {
            throw new Exception("❗ Логотип повинен бути кольоровим з підтримкою RGBA/BGRA.");
        }
        
        float scaleWidth = logoWidthPercent / 100f;
        float scaleHeight = logoHeightPercent / 100f;
        int logoWidth = (int)(qrBitmap.Width * scaleWidth);
        int logoHeight = (int)(qrBitmap.Height * scaleHeight);
        
        using var resizedLogo = new SKBitmap(logoWidth, logoHeight, logoBitmap.ColorType, logoBitmap.AlphaType);
        logoBitmap.ScalePixels(resizedLogo, SKFilterQuality.High);
        
        using var surface = SKSurface.Create(new SKImageInfo(qrBitmap.Width, qrBitmap.Height));
        var canvas = surface.Canvas;
        
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(qrBitmap, 0, 0);
        
        float centerX = (qrBitmap.Width - resizedLogo.Width) / 2f;
        float centerY = (qrBitmap.Height - resizedLogo.Height) / 2f;
        canvas.DrawBitmap(resizedLogo, new SKPoint(centerX, centerY));
        
        canvas.Flush();
        using var img = surface.Snapshot();
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }
}