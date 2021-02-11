using System;
using System.Windows;
using System.Windows.Forms;

namespace Snapper.Extensions
{
    public static class MyWpfExtensions
    {
        public static IWin32Window GetIWin32Window(this System.Windows.Media.Visual visual)
        {
            var source = PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            if (source != null)
            {
                IWin32Window win = new OldWindow(source.Handle);
                return win;
            }
            return null;
        }

        private class OldWindow : IWin32Window
        {
            private readonly IntPtr _handle;
            public OldWindow(IntPtr handle)
            {
                _handle = handle;
            }

            IntPtr IWin32Window.Handle
            {
                get { return _handle; }
            }
        }
    }
}