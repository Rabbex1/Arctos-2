using System.Runtime.InteropServices;
using System.Text;

namespace Arctos.Utils
{
  public static class Win32
  {
    #region External Native Functions
    //------------------------------------------------------------------------------------------------------------------------------------------
    [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Auto)]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", EntryPoint = "GetDC", CharSet = CharSet.Auto)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps", CharSet = CharSet.Auto)]
    public static extern int GetDeviceCaps(IntPtr hdc, int index);

    [DllImport("user32.dll", EntryPoint = "PhysicalToLogicalPoint", CharSet = CharSet.Auto)]
    public static extern bool PhysicalToLogicalPoint(IntPtr hWnd, out POINT p);

    [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
    public static extern bool SendMessage(IntPtr hWnd, uint msg, int wParam, StringBuilder lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wparam, int lparam);

    [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
    static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll", EntryPoint = "GetCursorPos", CharSet = CharSet.Auto)]
    public static extern bool GetCursorPos(out POINT p);

    #endregion External Native Functions

    #region Native Structs
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once InconsistentNaming
    public struct POINT
    {
      public int x;
      public int y;
    }
    #endregion Native Structs

    // ReSharper disable once InconsistentNaming
    public const int LOGPIXELSX = 88;    // Logical pixels/inch in X                 
                                         // ReSharper disable once InconsistentNaming
    public const int LOGPIXELSY = 90;    // Logical pixels/inch in Y                 
  }


}
