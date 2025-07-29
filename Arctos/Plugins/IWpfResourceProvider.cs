using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Plugins
{
    public interface IWpfResourceProvider
    {
        Uri ResourceUri { get; }
    }
}
