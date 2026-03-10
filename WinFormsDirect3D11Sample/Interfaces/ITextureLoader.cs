using System.Diagnostics;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.WIC;

namespace WinFormsDirect3D11Sample.Interfaces;

/// <summary>
/// Loads textures from files and creates Direct3D texture resources.
/// Uses composition and Dependency Injection for SOLID compliance.
/// </summary>
public interface ITextureLoader
{
    /// <summary>
    /// Loads a texture from file with optional resizing.
    /// </summary>
    ID3D11Texture2D LoadTexture(string fileName, int width = 0, int height = 0);
}
