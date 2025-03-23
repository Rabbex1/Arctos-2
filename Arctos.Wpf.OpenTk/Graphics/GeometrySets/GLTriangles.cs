using OpenTK.Graphics.OpenGL;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLTriangles : GLGeometrySet<GLVertex3>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public GLTriangles(bool isFilled)
        {
            PrimitiveType = isFilled ? PrimitiveType.Triangles : PrimitiveType.LineLoop;
            VertexCount = 3;
        }

        #endregion Constructor 
    }
}
