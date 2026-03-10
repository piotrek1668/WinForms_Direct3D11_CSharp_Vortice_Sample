using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.WIC;
using WinFormsDirect3D11Sample.Interfaces;

namespace WinFormsDirect3D11Sample;

/// <summary>
/// Validates formats against Direct3D device capabilities.
/// Single Responsibility: Only validates format compatibility.
/// </summary>
public class Direct3DFormatValidator : IFormatValidator
{
    private static class FormatDefaults
    {
        public const Format FallbackFormat = Format.R8G8B8A8_UNorm;
        public static Guid FallbackPixelFormat = PixelFormat.Format32bppRGBA;
        public const int FallbackBpp = 32;

        public const Format FloatFormat = Format.R32G32B32A32_Float;
        public static Guid FloatPixelFormat = PixelFormat.Format128bppRGBAFloat;
        public const int FloatBpp = 128;
    }

    private static readonly Dictionary<Guid, Guid> PixelFormatConversionMap = new()
    {
        { PixelFormat.FormatBlackWhite, PixelFormat.Format8bppGray },
        { PixelFormat.Format1bppIndexed, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format2bppIndexed, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format4bppIndexed, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format8bppIndexed, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format2bppGray, PixelFormat.Format8bppGray },
        { PixelFormat.Format4bppGray, PixelFormat.Format8bppGray },
        { PixelFormat.Format16bppGrayFixedPoint, PixelFormat.Format16bppGrayHalf },
        { PixelFormat.Format32bppGrayFixedPoint, PixelFormat.Format32bppGrayFloat },
        { PixelFormat.Format16bppBGR555, PixelFormat.Format16bppBGRA5551 },
        { PixelFormat.Format32bppBGR101010, PixelFormat.Format32bppRGBA1010102 },
        { PixelFormat.Format24bppBGR, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format24bppRGB, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format32bppPBGRA, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format32bppPRGBA, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format48bppRGB, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format48bppBGR, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format64bppBGRA, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format64bppPRGBA, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format64bppPBGRA, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format48bppRGBFixedPoint, PixelFormat.Format64bppRGBAHalf },
        { PixelFormat.Format48bppBGRFixedPoint, PixelFormat.Format64bppRGBAHalf },
        { PixelFormat.Format64bppRGBAFixedPoint, PixelFormat.Format64bppRGBAHalf },
        { PixelFormat.Format64bppBGRAFixedPoint, PixelFormat.Format64bppRGBAHalf },
        { PixelFormat.Format64bppRGBFixedPoint, PixelFormat.Format64bppRGBAHalf },
        { PixelFormat.Format64bppRGBHalf, PixelFormat.Format64bppRGBAHalf },
        { PixelFormat.Format48bppRGBHalf, PixelFormat.Format64bppRGBAHalf },
        { PixelFormat.Format128bppPRGBAFloat, PixelFormat.Format128bppRGBAFloat },
        { PixelFormat.Format128bppRGBFloat, PixelFormat.Format128bppRGBAFloat },
        { PixelFormat.Format128bppRGBAFixedPoint, PixelFormat.Format128bppRGBAFloat },
        { PixelFormat.Format128bppRGBFixedPoint, PixelFormat.Format128bppRGBAFloat },
        { PixelFormat.Format32bppRGBE, PixelFormat.Format128bppRGBAFloat },
        { PixelFormat.Format32bppCMYK, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format64bppCMYK, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format40bppCMYKAlpha, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format80bppCMYKAlpha, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format32bppRGB, PixelFormat.Format32bppRGBA },
        { PixelFormat.Format64bppRGB, PixelFormat.Format64bppRGBA },
        { PixelFormat.Format64bppPRGBAHalf, PixelFormat.Format64bppRGBAHalf }
    };

    private readonly ID3D11Device _device;
    private readonly IWICImagingFactory _wicFactory;

    public Direct3DFormatValidator(ID3D11Device device, IWICImagingFactory wicFactory)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(wicFactory);

        _device = device;
        _wicFactory = wicFactory;
    }

    public (Format FinalFormat, Guid ConvertGuid, int Bpp) ValidateAndAdjustFormat(
        Guid originalPixelFormat, Format proposedFormat, int originalBpp)
    {
        var format = proposedFormat;
        var convertGuid = originalPixelFormat;
        var bpp = originalBpp;

        // Handle special case: R32G32B32_Float might not support MipAutogen
        if (format == Format.R32G32B32_Float)
        {
            var fmtSupport = _device.CheckFormatSupport(Format.R32G32B32_Float);
            if (!fmtSupport.HasFlag(FormatSupport.MipAutogen))
            {
                format = FormatDefaults.FloatFormat;
                convertGuid = FormatDefaults.FloatPixelFormat;
                bpp = FormatDefaults.FloatBpp;
            }
        }

        // Verify format is supported for Texture2D operations
        var support = _device.CheckFormatSupport(format);
        if (!support.HasFlag(FormatSupport.Texture2D))
        {
            // Fallback to RGBA format supported by all devices
            format = FormatDefaults.FallbackFormat;
            convertGuid = FormatDefaults.FallbackPixelFormat;
            bpp = FormatDefaults.FallbackBpp;
        }

        return (format, convertGuid, bpp);
    }

    public static Guid GetConversionFormat(Guid originalFormat)
    {
        return PixelFormatConversionMap.TryGetValue(originalFormat, out var converted)
            ? converted
            : originalFormat;
    }
}
