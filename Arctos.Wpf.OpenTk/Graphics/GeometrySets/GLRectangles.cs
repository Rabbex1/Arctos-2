using OpenTK.Graphics.OpenGL;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLRectangles : GLGeometrySet<GLVertex4>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public GLRectangles(bool isFilled)
        {
            PrimitiveType = isFilled ? PrimitiveType.Triangles : PrimitiveType.LineLoop;
            VertexCount = 4;
        }

        #endregion Constructor 
        protected override void Draw()
        {
            if (PrimitiveType == PrimitiveType.Triangles)
            {
                if (Geometries.Count == 0)
                    return;
                uint restart = 50000;
                var indices = new uint[Geometries.Count * 6];
                uint index = 0;
                for (uint i = 0; i < Geometries.Count; i++)
                {
                    var iOffset = i * 6;
                    var vOffset = i * 4;
                    indices[iOffset] = vOffset;
                    indices[iOffset + 1] = vOffset + 1;
                    indices[iOffset + 2] = vOffset + 2;
                    indices[iOffset + 3] = vOffset;
                    indices[iOffset + 4] = vOffset + 2;
                    indices[iOffset + 5] = vOffset + 3;
                }
                GL.DrawElements(PrimitiveType, indices.Length, DrawElementsType.UnsignedInt, indices);
            }
            else
            {
                if (Geometries.Count == 0)
                    return;
                uint restart = 50000;
                var indices = new uint[Geometries.Count * 5 - 1];
                uint index = 0;
                for(uint i  = 0; i < indices.Length; i++)
                {
                    indices[i] = (i + 1)  % 5 == 0  ? restart : index++;
                }
                GL.PrimitiveRestartIndex(restart);
                GL.Enable(EnableCap.PrimitiveRestart);
                GL.DrawElements(PrimitiveType, indices.Length, DrawElementsType.UnsignedInt, indices);
            }
        }
    }

}

