using System.Numerics;
using System.Runtime.InteropServices;

namespace WinFormsDirect3D11Sample
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ColoredVertex {

        /// <summary>
        /// Gets or sets the position of the vertex.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the color of the vertex.
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredVertex"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="color">The color.</param>
        public ColoredVertex(Vector3 position, int color) : this() {
            Position = position;
            Color = color;
        }
    }
}
