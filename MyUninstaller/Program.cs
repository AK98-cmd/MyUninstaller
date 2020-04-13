using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//
using System.Threading;

namespace MyUninstaller
{
    static class Program
    {
        static Mutex m;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool first = false;
            m = new Mutex(true, Application.ProductName.ToString(), out first);
            if (first)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            else
            {
                MessageBox.Show("Application" + " " + Application.ProductName.ToString() + " " + "already running");
            }
        }
    }

    public static class DataContainer
    {
        public static bool SystemUpdates=false;
        public static bool SystemComponents=false;
        public static string CurrentLanguage="English";
        public static bool BuiltIn=false;
        public static bool MakeSystemRestorePoint = false;
        public static bool DeleteToTheRecycleBin = false;
    }
}