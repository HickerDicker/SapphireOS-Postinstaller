using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapphireOS_PostInstall.Classes
{
    internal class ScheduledTasks
    {
        private static readonly string[] Wildcards = {
            "update", "helloface", "customer experience improvement program", "microsoft compatibility appraiser",
            "startupapptask", "dssvccleanup", "bitlocker", "chkdsk", "data integrity scan", "defrag",
            "languagecomponentsinstaller", "upnp", "windows filtering platform", "systemrestore", "speech",
            "spaceport", "power efficiency", "cloudexperiencehost", "diagnosis", "file history",
            "bgtaskregistrationmaintenancetask", @"autochk\proxy", "siuf", "device information", "edp policy manager",
            "defender", "marebackup"
        };
        private static readonly string[] Exclusions = { "directx", "TPM" };
        private static List<string> GetAllScheduledTasks()
        {
            var psi = new ProcessStartInfo("schtasks.exe", "/query /fo csv /nh")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(line => line.Split(',')[0].Trim('"'))
                             .ToList();
            }
        }
        public static async Task DisableScheduledTasks()
        {
            var tasksToDisable = GetAllScheduledTasks()
                .Where(task => Wildcards.Any(w => task.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0) &&
                               !Exclusions.Any(e => task.IndexOf(e, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            if (!tasksToDisable.Any())
            {
                return;
            }

            Parallel.ForEach(tasksToDisable, task => RunTaskAction(task, enable: false));
        }
        private static void RunTaskAction(string taskName, bool enable)
        {
            string action = enable ? "Enabling" : "Disabling";
            string schtasksArg = enable ? "enable" : "disable";
            string arguments = $"/change /{schtasksArg} /TN \"{taskName}\"";
            Utils.RunWithMinSudo("schtasks.exe", arguments);
        }
    }
}
