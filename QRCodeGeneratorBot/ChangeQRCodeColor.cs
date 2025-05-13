using SkiaSharp;

public class ChangeQRCodeColor
{
    public SKBitmap ChangeColors(SKBitmap qrCodeImage, SKColor foregroundColor, SKColor backgroundColor)
    {
        if (qrCodeImage == null)
        {
            throw new ArgumentNullException(nameof(qrCodeImage), "QR-код не може бути null.");
        }

        var width = qrCodeImage.Width;
        var height = qrCodeImage.Height;
        
        var resultBitmap = new SKBitmap(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var pixel = qrCodeImage.GetPixel(x, y);
                
                if (pixel == SKColors.Black) 
                {
                    resultBitmap.SetPixel(x, y, foregroundColor);
                }
                else if (pixel == SKColors.White || pixel.Alpha == 0) 
                {
                    resultBitmap.SetPixel(x, y, backgroundColor);
                }
                else
                {
                  
                    resultBitmap.SetPixel(x, y, pixel);
                }
            }
        }
        return resultBitmap;
    }
}