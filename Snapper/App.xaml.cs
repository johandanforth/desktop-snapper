using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

//using Microsoft.Shell;

namespace Snapper
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] Arguments;

        private static readonly Mutex SnapperRunningMutex = new Mutex(true, "{1F6F0AC4-B1A2-21fd-A8CF-72F04E6BDE8F}");

        /// <summary>
        ///     Arguments: /minimized /show /host:hostname
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Arguments = e.Args;

            Directory.SetCurrentDirectory((Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                                           Path.GetDirectoryName(Assembly.GetAssembly(typeof(App)).CodeBase)) ??
                                          Path.GetFullPath(".")
            );

            if (SnapperRunningMutex.WaitOne(TimeSpan.Zero, true))
                //program is not started
                SnapperRunningMutex.ReleaseMutex();
            else
                //program is already running
                Current.Shutdown();

            base.OnStartup(e);
        }
    }
}