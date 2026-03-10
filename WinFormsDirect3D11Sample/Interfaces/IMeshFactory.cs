namespace WinFormsDirect3D11Sample.Interfaces;

/// <summary>
/// Defines a factory for creating mesh data.
/// </summary>
/// <typeparam name="TVertex">The vertex type</typeparam>
public interface IMeshFactory<TVertex> where TVertex : struct
{
    MeshData<TVertex> Create();
}
