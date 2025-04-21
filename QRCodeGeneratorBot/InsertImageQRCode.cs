using SkiaSharp;

public static class InsertImageQRCode
{
 
    public static byte[] AddLogoToQrCode(byte[] qrBytes, byte[] logoBytes, int logoWidthPercent, int logoHeightPercent)
    {
        using var qrBitmap = SKBitmap.Decode(qrBytes);
        if (qrBitmap == null) throw new ArgumentException("❗ Невірний формат QR-коду.");
        
        using var logoBitmap = SKBitmap.Decode(logoBytes);
        if (logoBitmap == null) throw new ArgumentException("❗ Невірний формат логотипа.");
        
        float scaleWidth = logoWidthPercent / 100f;
        float scaleHeight = logoHeightPercent / 100f;

        int logoWidth = (int)(qrBitmap.Width * scaleWidth);
        int logoHeight = (int)(qrBitmap.Height * scaleHeight);

        var resizedLogo = logoBitmap.Resize(new SKImageInfo(logoWidth, logoHeight), SKFilterQuality.High);
        
        using var canvas = new SKCanvas(qrBitmap);
        float centerX = (qrBitmap.Width - resizedLogo.Width) / 2f;
        float centerY = (qrBitmap.Height - resizedLogo.Height) / 2f;

        canvas.DrawBitmap(resizedLogo, new SKPoint(centerX, centerY));
        
        using var img = SKImage.FromBitmap(qrBitmap);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}