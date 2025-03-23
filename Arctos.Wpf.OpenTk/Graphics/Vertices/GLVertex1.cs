using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arctos.Wpf.OpenTk.Util;
using OpenTK.Mathematics;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLVertex1: IGLGeometry
    {
        public GLVertex V1 { get; }

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public GLVertex1(GLVertex v1)
        {
            V1 = v1;
        }


        #endregion Constructor

        public virtual GLVertex[] GetVertexData()
        {
            return new[]{V1};
        }
    }
}
