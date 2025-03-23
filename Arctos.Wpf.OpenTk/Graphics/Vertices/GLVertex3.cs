using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Wpf.OpenTk.Graphics
{
    public class GLVertex3 : GLVertex2
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public GLVertex V3 { get; }

        #endregion Properties
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public GLVertex3(GLVertex v1, GLVertex v2, GLVertex v3)
            : base(v1, v2)
        {
            V3 = v3;
        }


        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override GLVertex[] GetVertexData()
        {
            return new[] { V1, V2, V3 };
        }

        #endregion Methods
    }
}
