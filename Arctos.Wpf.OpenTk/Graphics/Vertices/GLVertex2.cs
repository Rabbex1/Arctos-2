using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLVertex2 : GLVertex1
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public GLVertex V2 { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public GLVertex2(GLVertex v1, GLVertex v2)
            :base(v1)
        {
            V2 = v2;
        }


        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override GLVertex[] GetVertexData()
        {
            return new[] { V1, V2 };
        }

        #endregion Methods
    }
}
