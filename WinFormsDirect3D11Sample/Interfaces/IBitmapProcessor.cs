using System.Diagnostics;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.WIC;

namespace WinFormsDirect3D11Sample.Interfaces;

/// <summary>
/// Handles pixel format conversion and bitmap processing.
/// Single Responsibility: Only processes and converts pixel data.
/// </summary>
public interface IBitmapProcessor
{
    /// <summary>
    /// Processes bitmap data with optional scaling and format conversion.
    /// </summary>
    byte[] ProcessBitmap(
        IWICBitmapFrameDecode frame,
        int targetWidth,
        int targetHeight,
        Guid targetPixelFormat,
        int rowPitch);
}
