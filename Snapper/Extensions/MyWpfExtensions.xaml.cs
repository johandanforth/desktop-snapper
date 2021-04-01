using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using IWin32Window = System.Windows.Forms.IWin32Window;

namespace Snapper.Extensions
{
    public static class MyWpfExtensions
    {
        public static IWin32Window GetIWin32Window(this Visual visual)
        {
            var source = PresentationSource.FromVisual(visual) as HwndSource;

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

            IntPtr IWin32Window.Handle => _handle;
        }
    }
}