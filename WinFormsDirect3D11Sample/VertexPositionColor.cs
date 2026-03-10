// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace WinFormsDirect3D11Sample;

public readonly struct VertexPositionColor(Vector3 position, Color4 color)
{
    public static readonly unsafe int SizeInBytes = sizeof(VertexPositionColor);

    public static readonly InputElementDescription[] InputElements = {
        new("POSITION", 0, Format.R32G32B32_Float, 0, 0),
        new("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
    };

    public readonly Vector3 Position = position;
    public readonly Color4 Color = color;
}
