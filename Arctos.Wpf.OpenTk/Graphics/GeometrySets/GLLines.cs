using OpenTK.Graphics.OpenGL;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLLines : GLGeometrySet<GLVertex2>
    {
        private readonly uint _pattern;

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor
        public GLLines(uint pattern = 0xffff)
        {
            PrimitiveType = PrimitiveType.Lines;
            VertexCount = 2;
            _pattern = pattern;
        }

        #endregion Constructor


        public override void Render(int program, double width, double height)
        {
            var locRes = GL.GetUniformLocation(program, "uResolution");
            var locPattern = GL.GetUniformLocation(program, "uPattern");
            var locFactor = GL.GetUniformLocation(program, "uFactor");

            GL.Uniform2(locRes, (float)width, (float)height);
            GL.Uniform1(locPattern, _pattern);
            GL.Uniform1(locFactor, 2.0f);

            base.Render(program, width, height);
        }

    }
}
