using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SapphireOS_PostInstall.Classes
{
    internal class Utils
    {
        public static void RunCommandSilent(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            Process process = new Process { StartInfo = startInfo };

            process.Start();
            process.WaitForExit();
        }
        internal static string GetBuildNumber()
        {
            return (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuild", "");
        }
        public static void RunWithMinSudo(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"C:\PostInstall\Tweaks\MinSudo.exe",
                Arguments = $"--TrustedInstaller --NoLogo {command} {arguments}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process process = new Process { StartInfo = startInfo };

            process.Start();
            process.WaitForExit();
        }
        public static byte[] HexToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }
        internal static int GetRAMInGB()
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            ulong bytes = (ulong)searcher.Get().Cast<ManagementObject>().First()["TotalPhysicalMemory"];
            return (int)Math.Ceiling(bytes / (1024.0 * 1024.0 * 1024.0));
        }
    }
}
