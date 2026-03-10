using System.ComponentModel;

namespace WinFormsDirect3D11Sample;

public class Camera
{
    [Category("Camera (Eye)")]
    public float EyeX { get; set; }

    [Category("Camera (Eye)")]
    public float EyeY { get; set; }

    [Category("Camera (Eye)")]
    public float EyeZ { get; set; } = 2;

    [Category("Camera (At)")]
    public float AtX { get; set; }

    [Category("Camera (At)")]
    public float AtY { get; set; }

    [Category("Camera (At)")]
    public float AtZ { get; set; }

    [Category("Camera (Up)")]
    public float UpX { get; set; }

    [Category("Camera (Up)")]
    public float UpY { get; set; } = 1;

    [Category("Camera (Up)")]
    public float UpZ { get; set; }
}
