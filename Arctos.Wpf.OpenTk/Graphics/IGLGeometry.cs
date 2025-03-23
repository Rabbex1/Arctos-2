using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arctos.Wpf.OpenTk.Util;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public interface IGLGeometry
    {
        GLVertex[] GetVertexData();
    }
}
