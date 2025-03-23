using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Arctos.Wpf.Sdx
{
  public class PZRCamera
  {
    public Vector2 Position {get; set;}
    public float Zoom {get; set;}
    public float Rotation {get; set;}
    public Rectangle Bounds {get; set;}

    public Matrix3x2 TransformMatrix => Matrix3x2.Translation(new Vector2(-Position.X, -Position.Y)) *
                Matrix3x2.Rotation(Rotation) *
                Matrix3x2.Scaling(Zoom) *
                Matrix3x2.Translation(new Vector2(Bounds.Width * 0.5f, Bounds.Height * 0.5f));


    public void Move(Vector2 displacement)
    {
      Position += displacement;
    }
  }
}
