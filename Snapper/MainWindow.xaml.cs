﻿using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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

        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern bool SetForegroundWindow(IntPtr hWnd);

        private DispatcherTimer _dispatcherTimer;
        private ClientIdleHandler _clientIdleHandler;
        private string _hostProcessName;
        //private readonly NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowLoaded;

            /*
            var icon = new Icon("Icons/SystemTray.ico");

            _notifyIcon = new NotifyIcon
                      {
                          Icon = icon,
                          Visible = true,
                      };
            _notifyIcon.DoubleClick += (s, a) =>
                                  {
                                      Show();
                                      WindowState = WindowState.Normal;
                                  };

            */
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            StartScreenShotTimer();
        }

        private void StartScreenShotTimer()
        {
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += TimerTick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, Settings.Default.ScreenShotsInterval);
            _dispatcherTimer.Start();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if(App.Command != null)
            {
                ProcessCommandLineArgs(App.Command, App.Argument);
            }
            
            //start client idle hook
            _clientIdleHandler = new ClientIdleHandler();
            _clientIdleHandler.Start();
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
            if(_hostProcessName != null)
            {
                var outlookRunning = Process.GetProcessesByName(_hostProcessName).Any();
                if (!outlookRunning)
                    this.Close();
            }

            if (_clientIdleHandler.IsActive)    //indicates user is active
            {
                //zero the idle counters
                _clientIdleHandler.IsActive = false;
                try
                {
                    var snapper = new ScreenSnapper();

                    if(Settings.Default.Screen == "Primary Screen")
                    {
                        snapper.SnapScreenAndSave(Settings.Default.ScreenShotsDirectory +
                            "/" + DateTime.Now.ToString("yyyy-MM-dd"), Screen.PrimaryScreen, ImageFormat.Jpeg, Settings.Default.ScreenShotsResolution);

                    }
                    else if(Settings.Default.Screen == "All Screens")
                    {
                        snapper.SnapAllScreensAndSave(Settings.Default.ScreenShotsDirectory +
                            "/" + DateTime.Now.ToString("yyyy-MM-dd"), ImageFormat.Jpeg, Settings.Default.ScreenShotsResolution);
                    }
                    else if(Settings.Default.Screen == "Active Window")
                    {
                        snapper.SnapActiveWindowAndSave(Settings.Default.ScreenShotsDirectory +
                            "/" + DateTime.Now.ToString("yyyy-MM-dd"), ImageFormat.Jpeg, Settings.Default.ScreenShotsResolution);
                    }
                    else
                    {
                        MessageBox.Show("Ops, kunde inte hämta upp vilken skärm du vill använda för screenshots, kör med 'Primary Screen' så länge. Ändra i settings om du vill :)");
                        Settings.Default.Screen = "Primary Screen";
                        Settings.Default.Save();
                    }

                }
                catch (Exception snapException)
                {
                    System.Windows.MessageBox.Show("Fel vid sparning av bild: " + snapException.Message);
                }

                Debug.Print(DateTime.Now + " - " + "Active");
            }
            else    //user was idle the last second
            {
                Debug.Print(DateTime.Now + " - " + "Idle");
            }
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

            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if(Settings.Default.AutoStart)
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
                    "Är du säker på att du vill avsluta programmet? Inga bilder kommer tas mer.",
                    "Stänga programmet?",
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

        public bool ProcessCommandLineArgs(string command, string argument)
        {
            StoreHostProcessName(argument);

            switch (command)
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

            return true;
        }

        private void StoreHostProcessName(string argument)
        {
            if (argument != null)
            {
                var arg = argument.ToLowerInvariant();
                if (arg.StartsWith("/host:"))
                {
                    if (arg.IndexOf(":", StringComparison.Ordinal) > 0)
                        _hostProcessName = arg.Substring(arg.IndexOf(":", StringComparison.Ordinal) + 1).Trim();
                }
            }
        }
    }
}