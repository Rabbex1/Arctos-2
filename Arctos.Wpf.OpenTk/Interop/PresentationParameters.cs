using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Wpf.OpenTk.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PresentationParameters
    {
        public int BackBufferWidth;
        public int BackBufferHeight;
        public Format BackBufferFormat;
        public uint BackBufferCount;
        public MultisampleType MultiSampleType;
        public int MultiSampleQuality;
        public SwapEffect SwapEffect;
        public IntPtr DeviceWindowHandle;
        public int Windowed;
        public int EnableAutoDepthStencil;
        public Format AutoDepthStencilFormat;
        public int Flags;
        public int FullScreen_RefreshRateInHz;
        public int PresentationInterval;
    }
}
