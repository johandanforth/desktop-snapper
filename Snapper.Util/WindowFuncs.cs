using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Snapper.Util
{
    public class WindowFuncs
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hWnd, out Rect rect);

        public ActiveWindowInfo GetActiveWindow()
        {
            var window = GetForegroundWindow();
            var length = GetWindowTextLength(window);
            var sb = new StringBuilder(length + 1);
            GetWindowText(window, sb, sb.Capacity);
            Rect rect;
            GetWindowRect(window, out rect);

            var bounds = new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);

            return new ActiveWindowInfo
            {
                ActiveProgramTitle = sb.ToString(),
                Bounds = bounds
            };
        }
    }
}