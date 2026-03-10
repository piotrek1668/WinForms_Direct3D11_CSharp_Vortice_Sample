using System.Numerics;
using Vortice.Mathematics;
using WinFormsDirect3D11Sample.Interfaces;

namespace WinFormsDirect3D11Sample.Factories;

/// <summary>
/// Factory for creating signal/waveform meshes.
/// </summary>
public class SignalMeshFactory : IMeshFactory<VertexPositionColor>
{
    private const float ZIndex = 0.0f;

    public MeshData<VertexPositionColor> Create()
    {
        var colorYellow = Colors.Black;
        var colorRed = Colors.Yellow;

        VertexPositionColor[] signalVertices = new[]
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

        return new MeshData<VertexPositionColor>(signalVertices);
    }
}
