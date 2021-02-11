using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
//using Microsoft.Shell;

namespace Snapper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static string Command;
        public static string Argument;

        //[STAThread]
        //private static void Main(string[] args)
        //{
        //    if (!SingleInstance<App>.InitializeAsFirstInstance("Snapper.App")) return;

        //    if (args.Length > 0)
        //        Command = args[0].ToLowerInvariant();
        //    if (args.Length > 1)
        //        Argument = args[1].ToLowerInvariant();

        //    var app = new App();
        //    app.InitializeComponent();
        //    app.Run();
        //    SingleInstance<App>.Cleanup();
        //}

        //bool ISingleInstanceApp.SignalExternalCommandLineArgs(IList<string> args)
        //{
        //    if (args.Count == 1)
        //        return ((MainWindow) MainWindow).ProcessCommandLineArgs(args[1].ToLowerInvariant(), null);
            
        //    if (args.Count > 1)
        //        return ((MainWindow) MainWindow).ProcessCommandLineArgs(args[1].ToLowerInvariant(), args[2]);

        //    return true;
            
        //}
    }
}
