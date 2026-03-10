using System.Diagnostics;
using Vortice.Direct3D11;
using WinFormsDirect3D11Sample.Interfaces;

namespace WinFormsDirect3D11Sample;

/// <summary>
/// Backward compatibility wrapper using TextureManager as a facade.
/// </summary>
public class TextureManager
{
    private readonly ITextureLoader _loader;

    public TextureManager(ID3D11Device device)
        : this(new WicTextureLoader(device))
    {
    }

    public TextureManager(ITextureLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);
        _loader = loader;
    }

    public ID3D11Texture2D? LoadTexture(string fileName, int width = 0, int height = 0)
    {
        try
        {
            return _loader.LoadTexture(fileName, width, height);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load texture '{fileName}': {ex.Message}");
            return null;
        }
    }
}