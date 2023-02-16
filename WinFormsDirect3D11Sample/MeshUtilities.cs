using System.Numerics;
using Vortice.Mathematics;

#nullable disable

namespace WinFormsDirect3D11Sample;

public class MeshData
{
    public readonly VertexPositionNormalTexture[] Vertices;
    public readonly VertexPositionColor[] VerticesColor;
    public readonly ushort[] Indices;

    public MeshData(VertexPositionColor[] vertices)
    {
        VerticesColor = vertices;
        Indices = null;
    }

    public MeshData(VertexPositionNormalTexture[] vertices, ushort[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }
}

public static class MeshUtilities
{
    private const int CubeFaceCount = 6;

    public static MeshData CreateCube(float size)
    {
        return MeshUtilities.CreateBox(new Vector3(size));
    }

    private static MeshData CreateBox(in Vector3 size)
    {
        List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
        List<ushort> indices = new List<ushort>();

        Vector3[] faceNormals = {
            Vector3.UnitZ, new Vector3(0.0f, 0.0f, -1.0f),
            Vector3.UnitX, new Vector3(-1.0f, 0.0f, 0.0f),
            Vector3.UnitY, new Vector3(0.0f, -1.0f, 0.0f),
        };

        Vector2[] textureCoordinates = {
            Vector2.UnitX,
            Vector2.One,
            Vector2.UnitY,
            Vector2.Zero,
        };

        Vector3 tsize = size / 2.0f;

        // Create each face in turn.
        int vbase = 0;
        for (int i = 0; i < MeshUtilities.CubeFaceCount; i++)
        {
            Vector3 normal = faceNormals[i];

            // Get two vectors perpendicular both to the face normal and to each other.
            Vector3 basis = (i >= 4) ? Vector3.UnitZ : Vector3.UnitY;

            Vector3 side1 = Vector3.Cross(normal, basis);
            Vector3 side2 = Vector3.Cross(normal, side1);

            // Six indices (two triangles) per face.
            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 1));
            indices.Add((ushort)(vbase + 2));

            indices.Add((ushort)(vbase + 0));
            indices.Add((ushort)(vbase + 2));
            indices.Add((ushort)(vbase + 3));

            // Four vertices per face.
            // (normal - side1 - side2) * tsize // normal // t0
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Subtract(Vector3.Subtract(normal, side1), side2), tsize),
                normal,
                textureCoordinates[0]
                ));

            // (normal - side1 + side2) * tsize // normal // t1
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Add(Vector3.Subtract(normal, side1), side2), tsize),
                normal,
                textureCoordinates[1]
                ));

            // (normal + side1 + side2) * tsize // normal // t2
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Add(normal, Vector3.Add(side1, side2)), tsize),
                normal,
                textureCoordinates[2]
                ));

            // (normal + side1 - side2) * tsize // normal // t3
            vertices.Add(new VertexPositionNormalTexture(
                Vector3.Multiply(Vector3.Subtract(Vector3.Add(normal, side1), side2), tsize),
                normal,
                textureCoordinates[3]
                ));

            vbase += 4;
        }

        return new MeshData(vertices.ToArray(), indices.ToArray());
    }

    public static MeshData CreateGrid()
    {
        var colorWhite = Colors.White;
        var offset = 1.0f;
        var lineVerticesBufferSize = 90;
        var lineVertices2 = new VertexPositionColor[lineVerticesBufferSize];
        var lineVertices3 = new VertexPositionColor[lineVerticesBufferSize];
        for (int i = 0; i < lineVerticesBufferSize; i++)
        {
            if ((i % 2) == 0)
            {
                var vertex1 = new VertexPositionColor(new Vector3(-1.0f, offset, 0.0f), colorWhite);
                var vertex2 = new VertexPositionColor(new Vector3(offset, -1.0f, 0.0f), colorWhite);
                lineVertices2[i] = vertex1;
                lineVertices3[i] = vertex2;
            }
            else
            {
                var vertex3 = new VertexPositionColor(new Vector3(1.0f, offset, 0.0f), colorWhite);
                var vertex4 = new VertexPositionColor(new Vector3(offset, 1.0f, 0.0f), colorWhite);
                lineVertices2[i] = vertex3;
                lineVertices3[i] = vertex4;

                offset -= 0.05f;
            }
        }

        var temp = lineVertices2.ToList();
        var temp2 = lineVertices3.ToList();
        temp.AddRange(temp2);

        return new MeshData(temp.ToArray());
    }

    public static MeshData CreateSignal()
    {
        var colorYellow = Colors.Black;
        var colorRed = Colors.Yellow;
        const float ZIndex = 0.0f;
        ReadOnlySpan<VertexPositionColor> signalVertices = new VertexPositionColor[]
        {
            new VertexPositionColor(new Vector3(-1.0f, -0.5f, ZIndex), colorYellow),
            new VertexPositionColor(new Vector3(-0.75f, -0.2f, ZIndex), colorYellow),
            new VertexPositionColor(new Vector3(-0.2f, -0.1f, ZIndex), colorYellow),
            new VertexPositionColor(new Vector3(-0.1f, 0.2f, ZIndex), colorYellow),
            new VertexPositionColor(new Vector3(0.2f, -0.25f, ZIndex), colorYellow),
            new VertexPositionColor(new Vector3(0.25f, 0.37f, ZIndex), colorRed),
            new VertexPositionColor(new Vector3(0.37f, -0.5f, ZIndex), colorRed),
            new VertexPositionColor(new Vector3(0.5f, -0.6f, ZIndex), colorRed),
            new VertexPositionColor(new Vector3(0.6f, 0.8f, ZIndex), colorRed),
            new VertexPositionColor(new Vector3(0.8f, 1.0f, ZIndex), colorRed)
        };

        return new MeshData(signalVertices.ToArray());
    }
}
