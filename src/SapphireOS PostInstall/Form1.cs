using Guna.UI2.WinForms;
using Microsoft.Win32;
using SapphireOS_PostInstall.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SapphireOS_PostInstall
{
    public partial class Form1 : Form
    {
        #region Fields
        private int _progressBarValue = 0;
        private System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
        private static RegistryKey scsi = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\SCSI", true);
        private static RegistryKey Audio = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio");
        static string build = Utils.GetBuildNumber();
        #endregion
        public Form1()
        {
            InitializeComponent();
            this.Shown += formShown;
        }

        private async void formShown(object sender, EventArgs e)
        {
            // These have a delay because c# execution is very fast and it'd just be very flashy to have all of this
            statusLabel.Text = "Status: Starting";
            await Task.Delay(100);
            statusLabel.Text = "Status: Opting out of Telemetry";
            await telemetry();
            updateProgressBar(5);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling Process Mitigations";
            await disableMitigations();
            updateProgressBar(10);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling Write Cache Buffering";
            await disableWriteCacheBuffer();
            updateProgressBar(15);
            await Task.Delay(100);
            statusLabel.Text = "Status: Editing Bcdedit";
            await editBcdedit();
            updateProgressBar(20);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling Powersavings";
            await disablePowerSavings();
            updateProgressBar(25);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling NetBios";
            Utils.RunWithMinSudo("cmd.exe", "/c for /f \"delims=\" %u in ('reg query \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\NetBT\\Parameters\\Interfaces\" /s /v \"NetbiosOptions\" 2^>nul ^| findstr \"HKEY\"') do reg add \"%u\" /v \"NetbiosOptions\" /t REG_DWORD /d 2 /f");
            updateProgressBar(30);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling Scheduled Tasks";
            await ScheduledTasks.DisableScheduledTasks();
            updateProgressBar(35);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling Exclusive Mode";
            await disableExclusiveMode();
            updateProgressBar(40);
            await Task.Delay(100);
            statusLabel.Text = "Status: Resetting Firewall Rules";
            Registry.LocalMachine.DeleteSubKeyTree(@"System\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules");
            Registry.LocalMachine.CreateSubKey(@"System\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules");
            updateProgressBar(45);
            await Task.Delay(100);
            statusLabel.Text = "Status: Removing Leftover Devices";
            Utils.RunCommandSilent(@"C:\PostInstall\Tweaks\DeviceCleanupCmd.exe", "* -s");
            updateProgressBar(50);
            await Task.Delay(100);
            statusLabel.Text = "Status: Network Tweaks";
            await networkTweaks();
            updateProgressBar(55);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling Devices";
            await disableDevices();
            updateProgressBar(60);
            await Task.Delay(100);
            statusLabel.Text = "Status: Tweaking Fsutil";
            Utils.RunCommandSilent("fsutil.exe", "behavior set disable8dot3 1");
            Utils.RunCommandSilent("fsutil.exe", "behavior set disablelastaccess 1");
            updateProgressBar(65);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling Driver Powersaving";
            Utils.RunCommandSilent("powershell.exe", "-Command \"Get-WmiObject MSPower_DeviceEnable -Namespace root\\wmi | ForEach-Object { $_.enable = $false; $_.psbase.put(); }\"");
            updateProgressBar(70);
            await Task.Delay(100);
            statusLabel.Text = "Status: Tweaking NIC";
            Utils.RunCommandSilent(@"C:\PostInstall\Others\Network\Run this if you had to install a network driver.bat", "");
            updateProgressBar(75);
            await Task.Delay(100);
            statusLabel.Text = "Status: Setting MSI Modes";
            await enableMSIModes();
            updateProgressBar(80);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling DMA Remapping";
            Utils.RunWithMinSudo("cmd.exe", "/c for /f \"delims=\" %b in ('reg query \"HKLM\\SYSTEM\\CurrentControlSet\\Services\" /s /v \"DmaRemappingCompatible\" 2^>nul ^| findstr \"HKEY\"') do reg add \"%b\" /v \"DmaRemappingCompatible\" /t REG_DWORD /d 0 /f");
            updateProgressBar(85);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling HIPM, DIPM and HDD Parking";
            foreach (string v in new[] { "EnableHIPM", "EnableDIPM", "EnableHDDParking" })
                Utils.RunWithMinSudo("cmd.exe", $"/c for /f \"delims=\" %b in ('reg query \"HKLM\\SYSTEM\\CurrentControlSet\\Services\" /s /v \"{v}\" 2^>nul ^| findstr \"HKEY\"') do reg add \"%b\" /v \"{v}\" /t REG_DWORD /d 0 /f");
            updateProgressBar(88);
            await Task.Delay(100);
            statusLabel.Text = "Status: Disabling StorPort Idle";
            Utils.RunWithMinSudo("cmd.exe", "/c for /f \"tokens=*\" %s in ('reg query \"HKLM\\SYSTEM\\CurrentControlSet\\Enum\" /s /f \"StorPort\" 2^>nul ^| findstr /e \"StorPort\"') do reg add \"%s\" /v \"EnableIdlePowerManagement\" /t REG_DWORD /d 0 /f");
            updateProgressBar(91);
            await Task.Delay(100);
            updateProgressBar(94);
            await Task.Delay(100);
            statusLabel.Text = "Status: Fixing Languages";
            Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\WindowsUpdate", "DoNotConnectToWindowsUpdateInternetLocations", 0, RegistryValueKind.DWord);
            Registry.SetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", "UseWUServer", 0, RegistryValueKind.DWord);
            updateProgressBar(97);
            await Task.Delay(100);
            if (Utils.GetRAMInGB() > 8)
            {
                statusLabel.Text = "Status: Disabling Memory Compression";
                Utils.RunWithMinSudo("powershell.exe", "Disable-MMAgent -MemoryCompression");
            }
            updateProgressBar(100);
            guna2Panel1.Controls.Add(EndScreen.Instance);
            EndScreen.Instance.Dock = DockStyle.Fill;
            EndScreen.Instance.BringToFront();
        }
        private void updateProgressBar(int newValue)
        {
            _progressBarValue = newValue;
            animationTimer.Interval = 15;
            animationTimer.Tick += animationTimerTick;
            animationTimer.Start();
        }

        private void animationTimerTick(object sender, EventArgs e)
        {
            if (scriptProgress.Value < _progressBarValue)
            {
                scriptProgress.Value += 1;
            }
            else if (scriptProgress.Value >= _progressBarValue)
            {
                scriptProgress.Value = _progressBarValue;
                animationTimer.Stop();
            }
        }
        #region Tweaks
        private async Task telemetry()
        {
            Utils.RunCommandSilent("cmd.exe", "/c setx DOTNET_TRY_CLI_TELEMETRY_OPTOUT 1");
            Utils.RunCommandSilent("cmd.exe", "/c setx POWERSHELL_TELEMETRY_OPTOUT 1");
            Utils.RunCommandSilent("cmd.exe", "/c setx DOTNET_CLI_TELEMETRY_OPTOUT 1");
            Utils.RunCommandSilent("cmd.exe", "/c setx DOCKER_CLI_TELEMETRY_OPTOUT 1");
            Utils.RunCommandSilent("cmd.exe", "/c setx npm_config_loglevel silent");
            Utils.RunCommandSilent("cmd.exe", "/c VS_TELEMETRY_OPT_OUT 1");
        }
        // I couldn't write these in native c# so you have to make due with it being done with minsudo keep in mind if I choose to not do it in a native way it is either because it was more convenient not to or I just couldn't like for example the kernel and ifeo
        private async Task disableMitigations()
        {
            Utils.RunCommandSilent("powershell.exe", "ForEach($v in (Get-Command -Name \"Set-ProcessMitigation\").Parameters[\"Disable\"].Attributes.ValidValues){Set-ProcessMitigation -SYSTEM -Disable $v.ToString() -ErrorAction SilentlyContinue}");

            string[] exes = {"fontdrvhost.exe", "dwm.exe", "lsass.exe", "svchost.exe","WmiPrvSE.exe", "winlogon.exe", "csrss.exe", "audiodg.exe","ntoskrnl.exe", "services.exe"};

            string modified = new string('2', 48);

            Utils.RunWithMinSudo("reg.exe", $"add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\kernel\" /v \"MitigationOptions\" /t REG_BINARY /d \"{modified}\" /f");
            Utils.RunWithMinSudo("reg.exe", $"add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\kernel\" /v \"MitigationAuditOptions\" /t REG_BINARY /d \"{modified}\" /f");

            foreach (string exe in exes)
            {
                Utils.RunWithMinSudo("reg.exe", $"add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\{exe}\" /v \"MitigationOptions\" /t REG_BINARY /d \"{modified}\" /f");
                Utils.RunWithMinSudo("reg.exe", $"add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\{exe}\" /v \"MitigationAuditOptions\" /t REG_BINARY /d \"{modified}\" /f");
            }
        }
        private async Task disableWriteCacheBuffer()
        {
            foreach (string device in scsi.GetSubKeyNames())
            {
                var deviceKey = scsi.OpenSubKey(device);
                foreach (string instance in deviceKey.GetSubKeyNames())
                {
                    string path = $"HKLM\\SYSTEM\\CurrentControlSet\\Enum\\SCSI\\{device}\\{instance}\\Device Parameters\\Disk";
                    Utils.RunWithMinSudo("reg.exe", $"add \"{path}\" /v \"CacheIsPowerProtected\" /t REG_DWORD /d \"1\" /f");
                    Utils.RunWithMinSudo("reg.exe", $"add \"{path}\" /v \"UserWriteCacheSetting\" /t REG_DWORD /d \"1\" /f");
                }
            }
        }
        private async Task editBcdedit()
        {
            Utils.RunCommandSilent("cmd.exe", "/c label C: SapphireOS");
            switch (build)
            {
                case "28000":
                    {
                        Utils.RunCommandSilent("bcdedit.exe", "/set {current} description SapphireOS 26H1");
                    }
                    break;
                case "26200":
                    {
                        Utils.RunCommandSilent("bcdedit.exe", "/set {current} description SapphireOS 25H2");
                    }
                    break;
                default:
                    {
                        Utils.RunCommandSilent("bcdedit.exe", "/set {current} description SapphireOS 23H2");
                    }
                    break;
            }
            Utils.RunCommandSilent("bcdedit.exe", "/set disabledynamictick yes");
            Utils.RunCommandSilent("bcdedit.exe", "/set bootmenupolicy legacy");
            Utils.RunCommandSilent("bcdedit.exe", "/set hypervisorlaunchtype off");
            Utils.RunCommandSilent("bcdedit.exe", "/set integrityservices disable");
            Utils.RunCommandSilent("bcdedit.exe", "/set isolatedcontext No");
            Utils.RunCommandSilent("bcdedit.exe", "/set vsmlaunchtype Off");
            Utils.RunCommandSilent("bcdedit.exe", "/set vm no");
            Utils.RunCommandSilent("bcdedit.exe", "/timeout 3");
        }
        private async Task disablePowerSavings()
        {
            int[] laptopChassis = { 8, 9, 10, 11, 12, 13, 14, 18, 21, 30, 31, 32 };
            var searcher = new System.Management.ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure");
            var chassis = (ushort[])searcher.Get().Cast<System.Management.ManagementObject>().First()["ChassisTypes"];
            bool isLaptop = laptopChassis.Contains((int)chassis[0]);

            if (isLaptop)
            {
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\serenum", "Start", 3, RegistryValueKind.DWord);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\sermouse", "Start", 3, RegistryValueKind.DWord);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\serial", "Start", 3, RegistryValueKind.DWord);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\wmiacpi", "Start", 2, RegistryValueKind.DWord);
                // trackpad fix
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\acpiex", "Start", 3, RegistryValueKind.DWord);
                // brightness slider fix
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\DisplayEnhancementService", "Start", 3, RegistryValueKind.DWord);
                // camera fix
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\ksthunk", "Start", 3, RegistryValueKind.DWord);
                // Fix the SapphireOS-Default-Services.reg
                string text = File.ReadAllText(@"C:\PostInstall\Services\SapphireOS-Default-Services.reg", Encoding.UTF8);
                text = Regex.Replace(text,@"(\[HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\acpiex\][^\[]*?""Start""=dword:)[0-9a-fA-F]{8}",m => m.Groups[1].Value + 3.ToString("x8"));
                text = Regex.Replace(text,@"(\[HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\DisplayEnhancementService\][^\[]*?""Start""=dword:)[0-9a-fA-F]{8}",m => m.Groups[1].Value + 3.ToString("x8"));
                text = Regex.Replace(text,@"(\[HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\ksthunk\][^\[]*?""Start""=dword:)[0-9a-fA-F]{8}",m => m.Groups[1].Value + 3.ToString("x8"));
                File.WriteAllText(@"C:\PostInstall\Services\SapphireOS-Default-Services.reg", text, Encoding.UTF8);
                Utils.RunWithMinSudo("cmd.exe /c", "reg add \"HKLM\\System\\CurrentControlSet\\Control\\Class\\{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}\" /v \"UpperFilters\" /t REG_MULTI_SZ /d \"ksthunk\" /f");
                Utils.RunWithMinSudo("cmd.exe /c", "reg add \"HKLM\\System\\CurrentControlSet\\Control\\Class\\{4D36E96C-E325-11CE-BFC1-08002BE10318}\" /v \"UpperFilters\" /t REG_MULTI_SZ /d \"ksthunk\" /f");
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Power\\PowerThrottling", "PowerThrottlingOff", 0, RegistryValueKind.DWord);
                // powerplans fix
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Power", "PlatformAoAcOverride", 0, RegistryValueKind.DWord);
                Utils.RunCommandSilent("powercfg.exe", "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e");
                Utils.RunCommandSilent("powercfg.exe", "/setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 4d2b0152-7d5c-498b-88e2-34345392a2c5 5000");
            }
            else
            {
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\DisplayEnhancementService", "Start", 4, RegistryValueKind.DWord);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\wmiacpi", "Start", 4, RegistryValueKind.DWord);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Power\\PowerThrottling", "PowerThrottlingOff", 1, RegistryValueKind.DWord);
                string[] powerValues = {"EnhancedPowerManagementEnabled", "AllowIdleIrpInD3", "EnableSelectiveSuspend","DeviceSelectiveSuspended", "SelectiveSuspendEnabled", "SelectiveSuspendOn","WaitWakeEnabled", "D3ColdSupported", "WdfDirectedPowerTransitionEnable","EnableIdlePowerManagement", "IdleInWorkingState"};

                foreach (string v in powerValues)
                    Utils.RunWithMinSudo("cmd.exe /c", "for /f \"delims=\" %b in ('reg query \"HKLM\\SYSTEM\\CurrentControlSet\\Enum\" /s /v \"" + v + "\" 2^>nul ^| findstr \"HKEY\"') do reg add \"%b\" /v \"" + v + "\" /t REG_DWORD /d \"0\" /f");
            }
        }
        private async Task disableExclusiveMode()
        {
            foreach (string type in new[] { "Capture", "Render" })
                foreach (string sub in Audio.OpenSubKey(type).GetSubKeyNames())
                    foreach (string prop in new[] { "3", "4" })
                        Utils.RunWithMinSudo("reg.exe", $"add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\MMDevices\\Audio\\{type}\\{sub}\\Properties\" /v \"{{b3f8fa53-0004-438e-9003-51a46e139bfc}},{prop}\" /t REG_DWORD /d 0 /f");
        }
        private async Task networkTweaks()
        {
            Utils.RunCommandSilent("netsh.exe", "int tcp set global dca=enabled");
            Utils.RunCommandSilent("netsh.exe", "int tcp set global netdma=enable");
            Utils.RunCommandSilent("netsh.exe", "interface isatap set state disable");
            Utils.RunCommandSilent("netsh.exe", "int tcp set global timestamps=disable");
            Utils.RunCommandSilent("netsh.exe", "int tcp set global rss=enabled");
            Utils.RunCommandSilent("netsh.exe", "int tcp set global nonsackrttresiliency=disable");
            Utils.RunCommandSilent("netsh.exe", "int tcp set global initialRto=2000");
            Utils.RunCommandSilent("netsh.exe", "int tcp set supplemental template=custom icw=10");
            Utils.RunCommandSilent("netsh.exe", "interface ip set interface ethernet currenthoplimit=6");
            Utils.RunCommandSilent("netsh.exe", "int ip set global taskoffload=enable");
        }
        private async Task disableDevices()
        {
            string[] devices = {
            "Microsoft Device Association Root Enumerator",
            "System Speaker",
            "Microsoft Radio Device Enumeration Bus",
            "PCI Encryption/Decryption Controller",
            "AMD PSP",
            "Intel SMBus",
            "Intel Management Engine",
            "PCI Memory Controller",
            "PCI standard RAM Controller",
            "System Timer",
            "WAN Miniport (IKEv2)",
            "WAN Miniport (IP)",
            "WAN Miniport (IPv6)",
            "WAN Miniport (L2TP)",
            "WAN Miniport (Network Monitor)",
            "WAN Miniport (PPPOE)",
            "WAN Miniport (PPTP)",
            "WAN Miniport (SSTP)",
            "Programmable Interrupt Controller",
            "Numeric Data Processor",
            "Communications Port (COM1)",
            "Microsoft RRAS Root Enumerator",
            "Microsoft GS Wavetable Synth"
            };

            foreach (string device in devices)
                Utils.RunCommandSilent("C:\\PostInstall\\Tweaks\\DevManView.exe", $"/disable \"{device}\"");
        }
        private async Task enableMSIModes()
        {
            string[] deviceTypes = new string[]
                {
                "Win32_USBController",
                "Win32_VideoController",
                "Win32_NetworkAdapter",
                "Win32_IDEController"
                };
            foreach (string deviceType in deviceTypes)
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT PNPDeviceID FROM {deviceType}");
                foreach (ManagementObject device in searcher.Get())
                {
                    string deviceID = device["PNPDeviceID"]?.ToString();
                    if (!string.IsNullOrEmpty(deviceID) && deviceID.StartsWith("PCI\\VEN_"))
                    {
                        string registryBasePath = $"SYSTEM\\CurrentControlSet\\Enum\\{deviceID}\\Device Parameters\\Interrupt Management";
                        string msiPath = $"{registryBasePath}\\MessageSignaledInterruptProperties";
                        Registry.SetValue($"HKEY_LOCAL_MACHINE\\{msiPath}", "MSISupported", 1, RegistryValueKind.DWord);
                        string priorityPath = $"{registryBasePath}\\Affinity Policy";
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(priorityPath, true))
                        {
                            if (key != null)
                            {
                                key.DeleteValue("DevicePriority", false);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
