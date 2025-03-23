using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLVertex4 : GLVertex3
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public GLVertex V4 { get; }

        #endregion Properties
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public GLVertex4(GLVertex v1, GLVertex v2, GLVertex v3, GLVertex v4)
            : base(v1, v2, v3)
        {
            V4 = v4;
        }


        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override GLVertex[] GetVertexData()
        {
            return new[] { V1, V2, V3, V4 };
        }

        #endregion Methods
    }
}
