using Vortice.DXGI;

namespace WinFormsDirect3D11Sample.Interfaces;

/// <summary>
/// Validates if a pixel format is supported by a Direct3D device.
/// Implements Dependency Inversion - abstracts device capability checking.
/// </summary>
public interface IFormatValidator
{
    /// <summary>
    /// Validates and potentially adjusts the format based on device capabilities.
    /// Returns the final format to use or throws if no suitable format is available.
    /// </summary>
    (Format FinalFormat, Guid ConvertGuid, int Bpp) ValidateAndAdjustFormat(
        Guid originalPixelFormat, Format proposedFormat, int originalBpp);
}
