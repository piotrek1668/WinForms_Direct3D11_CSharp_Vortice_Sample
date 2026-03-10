namespace WinFormsDirect3D11Sample;

/// <summary>
/// Generic mesh data container supporting different vertex types.
/// </summary>
/// <typeparam name="TVertex">The vertex type</typeparam>
public class MeshData<TVertex> where TVertex : struct
{
    public TVertex[] Vertices { get; }
    public ushort[]? Indices { get; }

    /// <summary>
    /// Creates mesh data with vertices and optional indices.
    /// </summary>
    public MeshData(TVertex[] vertices, ushort[]? indices = null)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        if (vertices.Length == 0)
            throw new ArgumentException("Vertices array cannot be empty", nameof(vertices));

        Vertices = vertices;
        Indices = indices;
    }
}
