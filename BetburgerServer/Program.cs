using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SeastoryServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            cServerSettings.GetInstance().LoadSettings();
            Application.Run(new frmMain());
        }
    }
}