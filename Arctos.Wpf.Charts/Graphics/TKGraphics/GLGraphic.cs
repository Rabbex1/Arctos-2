using System.Diagnostics;
using Arctos.Wpf.OpenTk.Control;
using OpenTK.Graphics.OpenGL;

namespace Arctos.Wpf.Charts
{
    public class GLGraphic : GLWpfControl
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected int Program;
        protected int ProgramPosition;
        protected int ProgramColor;


        protected string VertexShaderSource = @"#version 460 core

layout(location = 0) in vec3 iPosition;
layout(location = 1) in vec4 iColor;

uniform vec2 uViewport;

out vec4 color;

void main()
{
    //multiply y * -1 because wpf coords are top right not bottom right
    vec3 ndc = vec3((iPosition.x / uViewport.x) * 2 -1, ((iPosition.y / uViewport.y) * 2 -1) * -1, iPosition.z);
    gl_Position = vec4(ndc, 1);
    color = iColor;
}";

        protected string FragmentShaderSource = @"#version 460 core

in vec4 color;

out vec4 fragColor;

void main()
{
    fragColor = color;
}
";


        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Host
        private ICanvasHost? _host;
        public ICanvasHost? Host => _host ??= (Parent as GLCanvas)?.CanvasHost;

        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public GLGraphic()
        {
            Settings = new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 6,
                RenderContinuously = false,
                TransparentBackground = true
            };

        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public virtual void Initialize()
        {
            CreateProgram();
        }

        public override void Render(TimeSpan timeDelta)
        {
            throw new NotImplementedException();
        }

        private void CreateProgram()
        {
            Program = GL.CreateProgram();

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, VertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(vertexShader);
                Debug.WriteLine($"Vertex shader compile error: {log}");
            }

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, FragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(fragmentShader);
                Debug.WriteLine($"Fragment shader compile error: {log}");
            }

            GL.AttachShader(Program, vertexShader);
            GL.AttachShader(Program, fragmentShader);
            GL.LinkProgram(Program);
            GL.GetProgram(Program, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                var log = GL.GetProgramInfoLog(Program);
                Debug.WriteLine($"_program link error: {log}");
            }

            GL.DetachShader(Program, vertexShader);
            GL.DetachShader(Program, fragmentShader);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        #endregion Methods

        public void OnRender(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

        public override void Dispose()
        {
            GL.DeleteProgram(Program);
            base.Dispose();
        }

    }
}