using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SapphireOS_PostInstall.Dialogs;

namespace SapphireOS_PostInstall
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Check();
        }
        static void Check()
        {
            string manufacturer = null;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\OEMInformation"))
            {
                if (key != null)
                {
                    object value = key.GetValue("Manufacturer");
                    if (value is string stringValue)
                    {
                        manufacturer = stringValue;
                    }
                }
            }

            if (manufacturer != null && manufacturer.Equals("Hickensa", StringComparison.OrdinalIgnoreCase))
            {
                Application.Run(new Form1());
            }
            else
            {
                UnauthorizedDialog form = new UnauthorizedDialog();
                form.ShowDialog();
                Application.Exit();
            }
        }
    }
}
