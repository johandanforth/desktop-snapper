using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

using Snapper.Extensions;
using Snapper.Properties;
using Snapper.Util;

using Microsoft.Win32;

using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace Snapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private DispatcherTimer _dispatcherTimer;
        private ClientIdleHandler _clientIdleHandler;
        private string _hostProcessName;
        private readonly NotifyIcon _notifyIcon;

        protected override void OnClosing(CancelEventArgs e)
        {
            _notifyIcon.Dispose();

            base.OnClosing(e);
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowLoaded;

            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Icons/SystemTray.ico"),
                Visible = true,
                Text = "desktop-snapper"
            };
            _notifyIcon.DoubleClick += (s, a) =>
                                  {
                                      Show();
                                      WindowState = WindowState.Normal;
                                  };


            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            StartScreenShotTimer();
        }

        private void StartScreenShotTimer()
        {
            if (_dispatcherTimer != null )
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer = null;
            }

            _dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Settings.Default.ScreenShotsInterval)
            };
            _dispatcherTimer.Tick += TimerTick;
            _dispatcherTimer.Start();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (App.Arguments != null && App.Arguments.Length > 0)
            {
                ProcessCommandLineArgs(App.Arguments);
            }

            //start client idle hook
            _clientIdleHandler = new ClientIdleHandler();
#if NO_HOOKS
            //if debugging other parts of program and no hooks are necessary
#else
            _clientIdleHandler.Start();
#endif
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
            else
            {
                Show();
                Focus();
                Activate();
            }

            base.OnStateChanged(e);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Trace.TraceInformation(DateTime.Now + " tick...");
            
            if (_hostProcessName != null)
            {
                var outlookRunning = Process.GetProcessesByName(_hostProcessName).Any();
                if (!outlookRunning)
                    this.Close();
            }

#if !NO_HOOKS
            if (_clientIdleHandler.IsActive)    //indicates user is active
            {
                
                //zero the idle counters
                _clientIdleHandler.IsActive = false;
                _clientIdleHandler.Start();

#endif
                try
                {
                    var snapper = new ScreenSnapper();

                    if (Settings.Default.Screen == "Primary Screen")
                    {
                        snapper.SnapScreenAndSave(Settings.Default.ScreenShotsDirectory +
                            "/" + DateTime.Now.ToString("yyyy-MM-dd"), Screen.PrimaryScreen, ImageFormat.Jpeg,
                            Settings.Default.ScreenShotsResolution);

                    }
                    else if (Settings.Default.Screen == "All Screens")
                    {
                        snapper.SnapAllScreensAndSave(Settings.Default.ScreenShotsDirectory +
                            "/" + DateTime.Now.ToString("yyyy-MM-dd"), ImageFormat.Jpeg, Settings.Default.ScreenShotsResolution);
                    }
                    else if (Settings.Default.Screen == "Active Window")
                    {
                        snapper.SnapActiveWindowAndSave(Settings.Default.ScreenShotsDirectory +
                            "/" + DateTime.Now.ToString("yyyy-MM-dd"), ImageFormat.Jpeg, Settings.Default.ScreenShotsResolution);
                    }
                    else
                    {
                        MessageBox.Show("Ops, could not figure out which screen to use for screenshots, going with 'Primary Screen'.");
                        Settings.Default.Screen = "Primary Screen";
                        Settings.Default.Save();
                    }

                }
                catch (Exception snapException)
                {
                    System.Windows.MessageBox.Show("Exception while saving screenshot: " + snapException.Message);
                }

                Debug.Print(DateTime.Now + " - " + "Active");
#if !NO_HOOKS
            }
            else    //user was idle the last second
            {
                Debug.Print(DateTime.Now + " - " + "Idle");
            }
#endif
            CommandManager.InvalidateRequerySuggested();
        }

        private void GridMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void AppKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    WindowState = WindowState.Minimized;
                    return;
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Settings.Default.ScreenShotsDirectory))
                Directory.CreateDirectory(Settings.Default.ScreenShotsDirectory);

            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (Settings.Default.AutoStart)
            {
                rkApp.SetValue("Snapper", Assembly.GetExecutingAssembly().Location + " /minimized");
            }
            else
            {
                rkApp.DeleteValue("Snapper");
            }

            StartScreenShotTimer();

            Settings.Default.Save();

            WindowState = WindowState.Minimized;
        }

        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            var res =
                System.Windows.MessageBox.Show(
                    "Are you sure you want to exit the program? No more screenshots will be saved!",
                    "Close program?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

            if (res == MessageBoxResult.OK)
            {
                Close();
                return;
            }

        }

        private void PlayerButtonClick(object sender, RoutedEventArgs e)
        {
            var playerWindow = new PlayBackWindow();
            playerWindow.Show();
        }

        private void TextBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dlg = new FolderBrowserDialog
            {
                SelectedPath = Settings.Default.ScreenShotsDirectory
            };

            var result = dlg.ShowDialog(this.GetIWin32Window());
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.ScreenShotsDirectory = dlg.SelectedPath;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (_notifyIcon != null) _notifyIcon.Dispose();
        }

        public bool ProcessCommandLineArgs(string[] args)
        {
            StoreHostProcessName(args);

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "/show":
                        WindowState = WindowState.Normal;
                        Show();
                        Focus();
                        Activate();
                        break;
                    case "/minimized":
                        WindowState = WindowState.Minimized;
                        Hide();
                        break;
                }
            }


            return true;
        }

        private void StoreHostProcessName(string[] argument)
        {
            if (argument != null && argument.Length > 0)
            {
                foreach (var a in argument)
                {
                    var arg = a.ToLowerInvariant();
                    if (arg.StartsWith("/host:"))
                    {
                        if (arg.IndexOf(":", StringComparison.Ordinal) > 0)
                            _hostProcessName = arg.Substring(arg.IndexOf(":", StringComparison.Ordinal) + 1).Trim();
                    }
                }

            }
        }
    }
}
