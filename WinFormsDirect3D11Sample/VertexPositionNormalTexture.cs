// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace WinFormsDirect3D11Sample;

public readonly struct VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
{
    public static readonly unsafe int SizeInBytes = sizeof(VertexPositionNormalTexture);

    public static readonly InputElementDescription[] InputElements = {
        new("POSITION", 0, Format.R32G32B32_Float, 0, 0),
        new("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
        new("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
    };

    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;
    public readonly Vector2 TextureCoordinate = textureCoordinate;
}
