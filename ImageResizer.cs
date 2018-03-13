using System;

public class Resize
{
    private System.Drawing.Image resizedImage;
    private int _nSize;

	public Resize(System.Drawing.Image originalImage)
	{
        _nSize = 64;

        Rectangle destRect = new Rectangle(0, 0, _nSize, _nSize);
        Bitmap destImage = new Bitmap(_nSize, _nSize);

        destImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }
    }
}
