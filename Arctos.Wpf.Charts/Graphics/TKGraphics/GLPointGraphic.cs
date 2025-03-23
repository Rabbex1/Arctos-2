using System.Printing;
using System.Windows;
using System.Windows.Media;
using Arctos.Wpf.OpenTk.Graphics;
using Arctos.Wpf.OpenTk.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Arctos.Wpf.Charts
{
    public class GLPointGraphic : GLGraphic
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private IDataProvider? _dataProvider;
        private Color4 _color = new(0, 0, 1, 0);

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public GLPointGraphic()
        {

        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region Color
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(SolidColorBrush), 
            typeof(GLPointGraphic),
            new PropertyMetadata(default(SolidColorBrush), OnColorChanged)
            );
        public SolidColorBrush Color
        {
            get => (SolidColorBrush) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not GLPointGraphic graphic)
                return;
            if (e.NewValue is not SolidColorBrush colorBrush)
                return;
            graphic._color = new Color4(
                colorBrush.Color.R,
                colorBrush.Color.G,
                colorBrush.Color.B,
                colorBrush.Color.A
            );
        }
        #endregion

        #region PointSise
        public static readonly DependencyProperty PointSizeProperty = DependencyProperty.Register(
            "PointSize", typeof(float), typeof(GLPointGraphic), new PropertyMetadata(default(float)));

        public float PointSize
        {
            get => (float) GetValue(PointSizeProperty);
            set => SetValue(PointSizeProperty, value);
        }
        #endregion

        #endregion Dependency Properties

        public override void Render(TimeSpan timeDelta)
        {
            if (Host == null)
                return;
            if (_dataProvider == null)
            {
                _dataProvider ??= Host.DataProvider;
                if (_dataProvider == null)
                    return;
            }

            if (Host.PrimaryAxis is not { })
                return;
            if (Host.XAxis?.VisualRange is not VisualNumericRange xRange)
                return;
            if (Host.YAxis?.VisualRange is not VisualNumericRange yRange)
                return;

            var width = ActualWidth;
            var height = ActualHeight;


            var points = new GLPoints();

            _dataProvider.ForAxisRange(Host.PrimaryAxis, obj =>
            {
                if (obj is not DataPoint dataPoint)
                    return;
                //var x = MapToGLCoords((float)dataPoint.Angle, (float)xRange.Minimum, (float)xRange.Maximum, -1, 1, 6);
                //var y = MapToGLCoords((float)dataPoint.Value, (float)yRange.Minimum, (float)yRange.Maximum, -1, 1, 6);
                var x = (float)xRange.ValueToVisual(dataPoint.Angle);
                var y = (float)yRange.ValueToVisual(dataPoint.Value);
                points.Add(new GLVertex1
                (
                    new GLVertex(new Vector3(x, y, 0), _color)
                ));
            });

            GL.ClearColor(new Color4(0, 0, 0 ,0));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(Program);
            GL.PointSize(PointSize);

            points.Render(Program, width, height);


        }


    }
}
