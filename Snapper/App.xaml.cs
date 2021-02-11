using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
//using Microsoft.Shell;

namespace Snapper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        //public static string Command;
        //public static string Argument;
        public static string[] Arguments;

        static readonly Mutex SnapperRunningMutex = new Mutex(true, "{1F6F0AC4-B1A2-21fd-A8CF-72F04E6BDE8F}");

        protected override void OnStartup(StartupEventArgs e)
        {
            Arguments = e.Args;
            //var args = e.Args;

            //if (args.Length > 0)
            //    Command = args[0].ToLowerInvariant();
            //if (args.Length > 1)
            //    Argument = args[1].ToLowerInvariant();

            if (SnapperRunningMutex.WaitOne(TimeSpan.Zero, true))
            {
                //program is not started
                SnapperRunningMutex.ReleaseMutex();
            }
            else
            {
                //program is already running
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }

    }
}
