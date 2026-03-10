using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.WIC;
using WinFormsDirect3D11Sample.Interfaces;

namespace WinFormsDirect3D11Sample;

/// <summary>
/// Loads textures from disk using WIC and creates Direct3D resources.
/// Single Responsibility: Orchestrates texture loading.
/// Dependency Inversion: Depends on interfaces, not concrete implementations.
/// </summary>
public class WicTextureLoader : ITextureLoader
{
    private const string TextureSubDirectory = "Textures";

    private readonly ID3D11Device _device;
    private readonly IWICImagingFactory _wicFactory;
    private readonly IFormatValidator _formatValidator;
    private readonly IBitmapProcessor _bitmapProcessor;

    public WicTextureLoader(
        ID3D11Device device,
        IWICImagingFactory? wicFactory = null,
        IFormatValidator? formatValidator = null,
        IBitmapProcessor? bitmapProcessor = null)
    {
        ArgumentNullException.ThrowIfNull(device);

        _device = device;
        _wicFactory = wicFactory ?? new IWICImagingFactory();
        _formatValidator = formatValidator ?? new Direct3DFormatValidator(_device, _wicFactory);
        _bitmapProcessor = bitmapProcessor ?? new WicBitmapProcessor(_wicFactory);
    }

    public ID3D11Texture2D LoadTexture(string fileName, int width = 0, int height = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (width < 0 || height < 0)
            throw new ArgumentException("Dimensions cannot be negative");

        var textureFile = ResolveTextureFilePath(fileName);

        using var decoder = _wicFactory.CreateDecoderFromFileName(textureFile);
        using var frame = decoder.GetFrame(0);

        var frameSize = frame.Size;
        var targetWidth = width == 0 ? frameSize.Width : width;
        var targetHeight = height == 0 ? frameSize.Height : height;

        var (format, convertGuid, bpp) = DeterminePixelFormat(frame);
        var (finalFormat, finalConvertGuid, finalBpp) = _formatValidator.ValidateAndAdjustFormat(
            frame.PixelFormat, format, bpp);

        int rowPitch = (frameSize.Width * finalBpp + 7) / 8;

        var pixels = _bitmapProcessor.ProcessBitmap(
            frame, targetWidth, targetHeight, finalConvertGuid, rowPitch);

        return _device.CreateTexture2D(pixels, finalFormat, (uint)frameSize.Width, (uint)frameSize.Height)
            ?? throw new InvalidOperationException("Failed to create Direct3D texture");
    }

    private string ResolveTextureFilePath(string fileName)
    {
        var assetsPath = Path.Combine(AppContext.BaseDirectory, TextureSubDirectory);
        var textureFile = Path.Combine(assetsPath, fileName);

        if (!File.Exists(textureFile))
            throw new FileNotFoundException($"Texture file not found: {textureFile}");

        return textureFile;
    }

    private (Format Format, Guid ConvertGuid, int Bpp) DeterminePixelFormat(IWICBitmapFrameDecode frame)
    {
        var pixelFormat = frame.PixelFormat;
        var format = PixelFormat.ToDXGIFormat(pixelFormat);

        if (format != Format.Unknown)
        {
            // Handle BGRA to RGBA conversion
            if (pixelFormat == PixelFormat.Format32bppBGRA)
            {
                return (PixelFormat.ToDXGIFormat(PixelFormat.Format32bppRGBA),
                    PixelFormat.Format32bppRGBA,
                    32);
            }

            var bpp = (int)PixelFormat.WICBitsPerPixel(_wicFactory, pixelFormat);
            return (format, pixelFormat, bpp);
        }

        // Special case for 96bpp RGB
        if (pixelFormat == PixelFormat.Format96bppRGBFixedPoint)
        {
            return (Format.R32G32B32_Float, PixelFormat.Format96bppRGBFixedPoint, 96);
        }

        // Lookup conversion format
        var convertGuid = Direct3DFormatValidator.GetConversionFormat(pixelFormat);
        if (convertGuid != pixelFormat)
        {
            var convertedFormat = PixelFormat.ToDXGIFormat(convertGuid);
            if (convertedFormat != Format.Unknown)
            {
                var bpp = (int)PixelFormat.WICBitsPerPixel(_wicFactory, convertGuid);
                return (convertedFormat, convertGuid, bpp);
            }
        }

        throw new InvalidOperationException(
            $"WIC texture loader does not support pixel format {pixelFormat}");
    }
}
