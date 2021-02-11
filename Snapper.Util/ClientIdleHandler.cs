using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Snapper.Util
{
    public class ClientIdleHandler : IDisposable
    {
        public bool IsActive { get; set; }

        int _hHookKbd;
        int _hHookMouse;

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        public event HookProc MouseHookProcedure;
        public event HookProc KbdHookProcedure;

        //Use this function to install thread-specific hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
            IntPtr hInstance, int threadId);

        //Call this function to uninstall the hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        //Use this function to pass the hook information to next hook procedure in chain.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode,
            IntPtr wParam, IntPtr lParam);

        //Use this hook to get the module handle, needed for WPF environment
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public enum HookType : int
        {
            GlobalKeyboard = 13,
            GlobalMouse = 14
        }

        public int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //user is active, at least with the mouse
            IsActive = true;

            //just return the next hook
            return CallNextHookEx(_hHookMouse, nCode, wParam, lParam);
        }

        public int KbdHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //user is active, at least with the keyboard
            IsActive = true;

            //just return the next hook
            return CallNextHookEx(_hHookKbd, nCode, wParam, lParam);
        }

        public void Start()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            using (var mainModule = currentProcess.MainModule)
            {

                if (_hHookMouse == 0)
                {
                    // Create an instance of HookProc.
                    MouseHookProcedure = new HookProc(MouseHookProc);
                    // Create an instance of HookProc.
                    KbdHookProcedure = new HookProc(KbdHookProc);

                    //register a global hook
                    _hHookMouse = SetWindowsHookEx((int)HookType.GlobalMouse,
                                                  MouseHookProcedure,
                                                  GetModuleHandle(mainModule.ModuleName),
                                                  0);
                    if (_hHookMouse == 0)
                    {
                        Close();
                        throw new ApplicationException("SetWindowsHookEx() failed for the mouse");
                    }
                }

                if (_hHookKbd == 0)
                {
                    //register a global hook
                    _hHookKbd = SetWindowsHookEx((int)HookType.GlobalKeyboard,
                                                KbdHookProcedure,
                                                GetModuleHandle(mainModule.ModuleName),
                                                0);
                    if (_hHookKbd == 0)
                    {
                        Close();
                        throw new ApplicationException("SetWindowsHookEx() failed for the keyboard");
                    }
                }
            }
        }

        public void Close()
        {
            if (_hHookMouse != 0)
            {
                bool ret = UnhookWindowsHookEx(_hHookMouse);
                if (ret == false)
                {
                    throw new ApplicationException("UnhookWindowsHookEx() failed for the mouse");
                }
                _hHookMouse = 0;
            }

            if (_hHookKbd != 0)
            {
                bool ret = UnhookWindowsHookEx(_hHookKbd);
                if (ret == false)
                {
                    throw new ApplicationException("UnhookWindowsHookEx() failed for the keyboard");
                }
                _hHookKbd = 0;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_hHookMouse != 0 || _hHookKbd != 0)
                Close();
        }

        #endregion
    }
}