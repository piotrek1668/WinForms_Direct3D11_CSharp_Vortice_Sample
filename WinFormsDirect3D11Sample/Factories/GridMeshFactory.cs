using System.Numerics;
using Vortice.Mathematics;
using WinFormsDirect3D11Sample.Interfaces;

namespace WinFormsDirect3D11Sample.Factories;

/// <summary>
/// Factory for creating grid meshes.
/// </summary>
public class GridMeshFactory : IMeshFactory<VertexPositionColor>
{
    private const float LineOffsetDecrement = 0.05f;
    private const int LineVerticesBufferSize = 90;
    private const float InitialOffset = 1.0f;

    public MeshData<VertexPositionColor> Create()
    {
        var colorWhite = Colors.White;
        var lineVertices2 = new List<VertexPositionColor>(LineVerticesBufferSize);
        var lineVertices3 = new List<VertexPositionColor>(LineVerticesBufferSize);

        float offset = InitialOffset;
        for (int i = 0; i < LineVerticesBufferSize; i++)
        {
            if (i % 2 == 0)
            {
                lineVertices2.Add(new VertexPositionColor(new Vector3(-1.0f, offset, 0.0f), colorWhite));
                lineVertices3.Add(new VertexPositionColor(new Vector3(offset, -1.0f, 0.0f), colorWhite));
            }
            else
            {
                lineVertices2.Add(new VertexPositionColor(new Vector3(1.0f, offset, 0.0f), colorWhite));
                lineVertices3.Add(new VertexPositionColor(new Vector3(offset, 1.0f, 0.0f), colorWhite));
                offset -= LineOffsetDecrement;
            }
        }

        var combined = new List<VertexPositionColor>(lineVertices2.Count + lineVertices3.Count);
        combined.AddRange(lineVertices2);
        combined.AddRange(lineVertices3);

        return new MeshData<VertexPositionColor>(combined.ToArray());
    }
}
