// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace WinFormsDirect3D11Sample;

public readonly struct VertexPositionTexture
{
    public static readonly int SizeInBytes = Marshal.SizeOf(typeof(VertexPositionTexture));

    public static InputElementDescription[] InputElements = new[]
    {
        new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
        new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
    };

    public VertexPositionTexture(in Vector3 position, in Vector2 textCoord)
    {
        Position = position;
        TexCoord = textCoord;
    }

    public readonly Vector3 Position;
    public readonly Vector2 TexCoord;
}
