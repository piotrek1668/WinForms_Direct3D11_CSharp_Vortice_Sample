﻿// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace WinFormsDirect3D11Sample;

public readonly struct VertexPositionNormalTexture
{
    public static readonly unsafe int SizeInBytes = sizeof(VertexPositionNormalTexture);

    public static readonly InputElementDescription[] InputElements = {
        new("POSITION", 0, Format.R32G32B32_Float, 0, 0),
        new("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
        new("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
    };

    public VertexPositionNormalTexture(in Vector3 position, in Vector3 normal, in Vector2 textureCoordinate)
    {
        Position = position;
        Normal = normal;
        TextureCoordinate = textureCoordinate;
    }

    public readonly Vector3 Position;
    public readonly Vector3 Normal;
    public readonly Vector2 TextureCoordinate;
}
