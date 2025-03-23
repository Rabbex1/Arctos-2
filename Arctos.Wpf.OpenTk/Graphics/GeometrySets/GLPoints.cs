using OpenTK.Graphics.OpenGL;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLPoints : GLGeometrySet<GLVertex1>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public GLPoints()
        {
            PrimitiveType = PrimitiveType.Points;
            VertexCount = 1;
        }

        #endregion Constructor
    }
}
