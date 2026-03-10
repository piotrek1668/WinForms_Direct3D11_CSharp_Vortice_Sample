using WinFormsDirect3D11Sample.Factories;

namespace WinFormsDirect3D11Sample;

/// <summary>
/// Static utility factory methods for mesh creation (backward compatibility).
/// </summary>
public static class MeshUtilities
{
    public static MeshData<VertexPositionNormalTexture> CreateCube(float size)
    {
        return new CubeMeshFactory(size).Create();
    }

    public static MeshData<VertexPositionColor> CreateGrid()
    {
        return new GridMeshFactory().Create();
    }

    public static MeshData<VertexPositionColor> CreateSignal()
    {
        return new SignalMeshFactory().Create();
    }
}
