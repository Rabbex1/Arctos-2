using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Wpf.Sdx
{
  public class PZRD2DControl : D2DControl
  {
    PZRCamera Camera;

    public override void Render(RenderTarget target)
    {
      D2DRenderTarget.Transform = Camera.TransformMatrix;
    }
  }
}
