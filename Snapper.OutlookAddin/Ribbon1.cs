using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Tools.Ribbon;

using Snapper;

namespace OutlookAddIn1
{
    public partial class Ribbon1
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
        }

        private void PlayerButtonClick(object sender, RibbonControlEventArgs e)
        {
            var calendarView = ThisAddIn._application.ActiveExplorer().CurrentView as _CalendarView;

            if (calendarView != null)
            {
                var dateString = calendarView.SelectedStartTime.ToString("yyyy-MM-dd");
                var playerWindow = new PlayBackWindow();
                playerWindow.Show();
                playerWindow.ShowDate(dateString);
            }
        }

        private void SettingsButtonClick(object sender, RibbonControlEventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("Snapper"))
            {
                Debug.Print("Killing " + process.ProcessName);
                process.Kill();
            }

            var processName = Process.GetCurrentProcess().ProcessName;
            Debug.Print("Starting adddin from " + processName);

            var addinAssembly = Assembly.GetExecutingAssembly();
            var asmPath = Path.GetDirectoryName(addinAssembly.CodeBase);
            var path = new Uri(asmPath + "/Snapper.exe");
            var myProcess = new Process();
            myProcess.StartInfo.FileName = path.AbsoluteUri;
            myProcess.StartInfo.Arguments = "/show /host:" + processName;
            myProcess.Start();
        }
    }
}