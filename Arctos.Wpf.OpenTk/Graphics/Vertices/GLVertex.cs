using OpenTK.Mathematics;
using Color4 = OpenTK.Mathematics.Color4;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public struct GLVertex
    {
        public Vector3 Position;
        public Color4 Color;

        public GLVertex(Vector3 position, Color4 color)
        {
            Position = position;
            Color = color;
        }
    }
}
