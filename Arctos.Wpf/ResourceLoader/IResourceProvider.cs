using System;
using System.Collections.Generic;

namespace Arctos.Wpf
{
    public interface IResourceProvider
    {
        IEnumerable<Uri> GetResourceUris();
    }
}
