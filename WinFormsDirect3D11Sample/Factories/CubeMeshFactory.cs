using System.Numerics;
using WinFormsDirect3D11Sample.Interfaces;

namespace WinFormsDirect3D11Sample.Factories;

/// <summary>
/// Factory for creating cube meshes.
/// </summary>
public class CubeMeshFactory : IMeshFactory<VertexPositionNormalTexture>
{
    private const int CubeFaceCount = 6;
    private readonly float _size;

    public CubeMeshFactory(float size)
    {
        if (size <= 0)
            throw new ArgumentException("Size must be positive", nameof(size));
        _size = size;
    }

    public MeshData<VertexPositionNormalTexture> Create()
    {
        var vertices = new List<VertexPositionNormalTexture>();
        var indices = new List<ushort>();

        Vector3[] faceNormals =
        {
            Vector3.UnitZ, new Vector3(0.0f, 0.0f, -1.0f),
            Vector3.UnitX, new Vector3(-1.0f, 0.0f, 0.0f),
            Vector3.UnitY, new Vector3(0.0f, -1.0f, 0.0f),
        };

        Vector2[] textureCoordinates =
        {
            Vector2.UnitX,
            Vector2.One,
            Vector2.UnitY,
            Vector2.Zero,
        };

        Vector3 tsize = new Vector3(_size) / 2.0f;

        int vbase = 0;
        for (int i = 0; i < CubeFaceCount; i++)
        {
            Vector3 normal = faceNormals[i];
            Vector3 basis = i >= 4 ? Vector3.UnitZ : Vector3.UnitY;

            Vector3 side1 = Vector3.Cross(normal, basis);
            Vector3 side2 = Vector3.Cross(normal, side1);

            // Six indices (two triangles) per face
            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 1));
            indices.Add((ushort)(vbase + 2));

            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 2));
            indices.Add((ushort)(vbase + 3));

            // Four vertices per face
            vertices.Add(new VertexPositionNormalTexture(
                (normal - side1 - side2) * tsize,
                normal,
                textureCoordinates[0]));

            vertices.Add(new VertexPositionNormalTexture(
                (normal - side1 + side2) * tsize,
                normal,
                textureCoordinates[1]));

            vertices.Add(new VertexPositionNormalTexture(
                (normal + side1 + side2) * tsize,
                normal,
                textureCoordinates[2]));

            vertices.Add(new VertexPositionNormalTexture(
                (normal + side1 - side2) * tsize,
                normal,
                textureCoordinates[3]));

            vbase += 4;
        }

        return new MeshData<VertexPositionNormalTexture>(vertices.ToArray(), indices.ToArray());
    }
}
