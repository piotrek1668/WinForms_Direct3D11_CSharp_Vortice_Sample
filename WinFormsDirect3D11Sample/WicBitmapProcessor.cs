using Vortice.WIC;
using WinFormsDirect3D11Sample.Interfaces;

namespace WinFormsDirect3D11Sample;

/// <summary>
/// Processes bitmaps using WIC (Windows Imaging Component).
/// </summary>
public class WicBitmapProcessor : IBitmapProcessor
{
    private readonly IWICImagingFactory _wicFactory;

    public WicBitmapProcessor(IWICImagingFactory wicFactory)
    {
        ArgumentNullException.ThrowIfNull(wicFactory);
        _wicFactory = wicFactory;
    }

    public byte[] ProcessBitmap(
        IWICBitmapFrameDecode frame,
        int targetWidth,
        int targetHeight,
        Guid targetPixelFormat,
        int rowPitch)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (targetWidth <= 0 || targetHeight <= 0)
            throw new ArgumentException("Target dimensions must be positive");

        var sourceSize = frame.Size;
        var sourcePixelFormat = frame.PixelFormat;
        var sizeInBytes = rowPitch * targetHeight;
        var pixels = new byte[sizeInBytes];

        // No conversion or resize needed
        if (targetPixelFormat == sourcePixelFormat &&
            sourceSize.Width == targetWidth &&
            sourceSize.Height == targetHeight)
        {
            frame.CopyPixels((uint)rowPitch, pixels);
            return pixels;
        }

        // Resize needed
        if (sourceSize.Width != targetWidth || sourceSize.Height != targetHeight)
        {
            return ProcessWithResize(frame, targetWidth, targetHeight, targetPixelFormat, rowPitch);
        }

        // Format conversion only
        return ProcessFormatConversion(frame, targetPixelFormat, rowPitch);
    }

    private byte[] ProcessWithResize(
        IWICBitmapFrameDecode frame,
        int targetWidth,
        int targetHeight,
        Guid targetPixelFormat,
        int rowPitch)
    {
        using var scaler = _wicFactory.CreateBitmapScaler();
        scaler.Initialize(frame, (uint)targetWidth, (uint)targetHeight, BitmapInterpolationMode.Fant);

        var scalerPixelFormat = scaler.PixelFormat;
        var sizeInBytes = rowPitch * targetHeight;
        var pixels = new byte[sizeInBytes];

        if (targetPixelFormat == scalerPixelFormat)
        {
            scaler.CopyPixels((uint)rowPitch, pixels);
            return pixels;
        }

        return ConvertFormat(scaler, targetPixelFormat, rowPitch);
    }

    private byte[] ProcessFormatConversion(
        IWICBitmapFrameDecode frame,
        Guid targetPixelFormat,
        int rowPitch)
    {
        var sourcePixelFormat = frame.PixelFormat;
        var size = frame.Size;
        var sizeInBytes = rowPitch * size.Height;
        var pixels = new byte[sizeInBytes];

        return ConvertFormat(frame, targetPixelFormat, rowPitch);
    }

    private byte[] ConvertFormat(
        IWICBitmapSource source,
        Guid targetPixelFormat,
        int rowPitch)
    {
        using var converter = _wicFactory.CreateFormatConverter();
        var sourcePixelFormat = source.PixelFormat;

        if (!converter.CanConvert(sourcePixelFormat, targetPixelFormat))
            throw new InvalidOperationException(
                $"Cannot convert from {sourcePixelFormat} to {targetPixelFormat}");

        converter.Initialize(source, targetPixelFormat, BitmapDitherType.ErrorDiffusion,
            null, 0, BitmapPaletteType.MedianCut);

        var size = source.Size;
        var sizeInBytes = rowPitch * size.Height;
        var pixels = new byte[sizeInBytes];
        converter.CopyPixels((uint)rowPitch, pixels);

        return pixels;
    }
}
