using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

#pragma warning disable CA1416 // Warning for reachable on all platforms.

public class ScreenCapture
{
    // Via https://stackoverflow.com/a/24879511/34170

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetDesktopWindow();

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }   

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    public static Image CaptureDesktop(int x1, int y1, int x2, int y2)
    {
        return CaptureWindow(GetDesktopWindow(), x1, y1, x2, y2);
    }

    public static Bitmap CaptureActiveWindow(int x1, int y1, int x2, int y2)
    {
        return CaptureWindow(GetForegroundWindow(), x1, y1, x2, y2);
    }

    public static Bitmap CaptureWindow(IntPtr handle, int x1, int y1, int x2, int y2)
    {
        var rect = new Rect();
        GetWindowRect(handle, ref rect);
        int width = x2 - x1;
        int height = y2 - y1;        
        var result = new Bitmap(width, height);

        using (var graphics = Graphics.FromImage(result))
        {
            var bounds = new Rectangle(x1, y1, width, height);
            graphics.CopyFromScreen(rect.Left + x1, rect.Top + y1, 0, 0, bounds.Size);
        }

        return result;
    }
}