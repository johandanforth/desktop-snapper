using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using Application = Microsoft.Office.Interop.Outlook.Application;

namespace OutlookAddIn1
{
    public partial class ThisAddIn
    {
        public static Application _application;


        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            _application = this.Application;

            var processName = Process.GetCurrentProcess().ProcessName;
            Debug.Print("Starting adddin from " + processName);

            var addinAssembly = Assembly.GetExecutingAssembly();
            var asmPath = Path.GetDirectoryName(addinAssembly.CodeBase);
            var path = new Uri(asmPath + "/Snapper.exe");
            var myProcess = new Process();
            myProcess.StartInfo.FileName = path.AbsoluteUri;
            myProcess.StartInfo.Arguments = "/minimized /host:" + processName;
            myProcess.Start();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            Startup += ThisAddIn_Startup;
            Shutdown += ThisAddIn_Shutdown;
        }

        #endregion
    }
}
