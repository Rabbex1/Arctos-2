using System.Windows;
using Arctos.Wpf.OpenTk.Graphics;
using Arctos.Wpf.OpenTk.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Arctos.Wpf.Charts
{
    public class GLGridGraphic : GLGraphic
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private float _strokeThickness = 1.0f;
        private Color4 _stroke = new(0,0,1,0);

        #endregion Fields


        #region LineStyle
        public static readonly DependencyProperty LineStyleProperty = DependencyProperty.Register
        (
          "LineStyle",
          typeof(Style),
          typeof(GLGridGraphic),
          new PropertyMetadata(default(Style), OnLineStyleChanged)
        );

        public Style LineStyle
        {
            get => (Style)GetValue(LineStyleProperty);
            set => SetValue(LineStyleProperty, value);
        }

        private static void OnLineStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not GLGridGraphic graphic)
                return;
            var style = e.NewValue as Style;
            if (style?.TargetType != typeof(System.Windows.Shapes.Line))
                return;
            var stroke = (style.Setters.FirstOrDefault(s => ((Setter)s).Property.Name == "Stroke") as Setter)?.Value;
            if (stroke is System.Windows.Media.SolidColorBrush colorBrush)
                graphic._stroke = new Color4(
                    colorBrush.Color.R,
                    colorBrush.Color.G,
                    colorBrush.Color.B,
                    colorBrush.Color.A
                );
            var strokeThickness = (style.Setters.FirstOrDefault(s => ((Setter)s).Property.Name == "StrokeThickness") as Setter)?.Value;
            if (strokeThickness is double thickness)
                graphic._strokeThickness = (float)thickness;
        }
        #endregion

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override void Initialize()
        {
            VertexShaderSource = @"#version 460 core

layout(location = 0) in vec3 iPosition;
layout(location = 1) in vec4 iColor;

uniform vec2 uViewport;

out vec4 color;
flat out vec3 startPos;
    out vec3 vertPos;

uniform mat4 u_mvp;

void main()
{
    //multiply y * -1 because wpf coords are top right not bottom right
    vec3 ndc = vec3((iPosition.x / uViewport.x) * 2 -1, ((iPosition.y / uViewport.y) * 2 -1) * -1, iPosition.z);
    vec4 pos = vec4(ndc, 1);
    gl_Position = pos;
    vertPos = pos.xyz / pos.w;
    startPos = vertPos;
    color = iColor;
}";

            FragmentShaderSource = @"#version 460 core

flat in vec3 startPos;
in vec3 vertPos;
in vec4 color;

out vec4 fragColor;

uniform vec2  uResolution;
uniform uint  uPattern;
uniform float uFactor;

void main()
{
    vec2  dir  = (vertPos.xy-startPos.xy) * uResolution/2.0;
    float dist = length(dir);

    uint bit = uint(round(dist / uFactor)) & 15U;
    if ((uPattern & (1U<<bit)) == 0U)
    {
        discard; 
    }
    fragColor = color;
}";

            base.Initialize();
        }

        public override void Render(TimeSpan timeDelta)
        {
            if (Host == null)
                return;

            var lines = new GLLines(0x5555);

            var width = ActualWidth;
            var height = ActualHeight;


            if (Host.YAxis != null)
            {
                foreach (var tickSet in Host.YAxis.TickSets)
                {
                    if (tickSet.IsGridLine)
                    {
                        foreach (Tick tick in tickSet.Children)
                        {
                            if (!double.IsNaN(tick.Center.Y))
                            {

                                lines.Add(new GLVertex2
                                (
                                    new GLVertex(new Vector3(0, (float)tick.Center.Y, -0.1f), _stroke),
                                    new GLVertex(new Vector3((float)width, (float)tick.Center.Y, -0.1f), _stroke)
                                ));
                            }
                        }
                    }
                }

            }
            if (Host.XAxis != null)
            {
                foreach (var tickSet in Host.XAxis.TickSets)
                {
                    if (tickSet.IsGridLine)
                    {
                        foreach (Tick tick in tickSet.Children)
                        {
                            if (!double.IsNaN(tick.Center.X))
                            {

                                lines.Add(new GLVertex2
                                (
                                     new GLVertex(new Vector3((float)tick.Center.X, 0, -0.1f), _stroke),
                                     new GLVertex(new Vector3((float)tick.Center.X, (float)height, -0.1f), _stroke)
                                ));
                            }
                        }
                    }
                }

            }


            GL.ClearColor(new Color4(0, 0, 0, 0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(Program);
            GL.LineWidth(_strokeThickness);

            lines.Render(Program, width, height);
        }
        #endregion Methods
    }
}
