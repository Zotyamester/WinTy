using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;

namespace WinTy
{
    public delegate void Method();

    public class UtilityMethods
    {
        public static void EditRegistry()
        {
            RegistryKey key;

            key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", true);
            if (key != null)
            {
                key.SetValue("AllowTelemetry", 0x0000000c, RegistryValueKind.DWord);
            }
        }

        public static void EditHosts()
        {
            List<string> BlackList = new List<string>()
            {
                "vortex.data.microsoft.com", "vortex-win.data.microsoft.com",
                "telecommand.telemetry.microsoft.com", "telecommand.telemetry.microsoft.com.nsatc.net",
                "oca.telemetry.microsoft.com", "oca.telemetry.microsoft.com.nsatc.net",
                "sqm.telemetry.microsoft.com", "sqm.telemetry.microsoft.com.nsatc.net",
                "watson.telemetry.microsoft.com", "watson.telemetry.microsoft.com.nsatc.net",
                "redir.metaservices.microsoft.com", "choice.microsoft.com",
                "choice.microsoft.com.nsatc.net", "df.telemetry.microsoft.com",
                "reports.wes.df.telemetry.microsoft.com", "wes.df.telemetry.microsoft.com",
                "services.wes.df.telemetry.microsoft.com", "sqm.df.telemetry.microsoft.com",
                "telemetry.microsoft.com", "watson.ppe.telemetry.microsoft.com",
                "telemetry.appex.bing.net", "telemetry.urs.microsoft.com",
                "telemetry.appex.bing.net:443", "settings-sandbox.data.microsoft.com",
                "vortex-sandbox.data.microsoft.com", "survey.watson.microsoft.com",
                "watson.live.com", "watson.microsoft.com",
                "statsfe2.ws.microsoft.com", "corpext.msitadfs.glbdns2.microsoft.com",
                "compatexchange.cloudapp.net", "cs1.wpc.v0cdn.net", "a-0001.a-msedge.net",
                "statsfe2.update.microsoft.com.akadns.net", "sls.update.microsoft.com.akadns.net",
                "fe2.update.microsoft.com.akadns.net", "65.55.108.23",
                "65.39.117.230", "23.218.212.69",
                "134.170.30.202", "137.116.81.24",
                "diagnostics.support.microsoft.com", "corp.sts.microsoft.com",
                "statsfe1.ws.microsoft.com", "pre.footprintpredict.com",
                "204.79.197.200", "23.218.212.69",
                "i1.services.social.microsoft.com", "i1.services.social.microsoft.com.nsatc.net",
                "feedback.windows.com", "feedback.microsoft-hohm.com",
                "feedback.search.microsoft.com"
            };

            using (FileStream stream = new FileStream(@"C:\Windows\System32\drivers\etc\hosts", FileMode.Append, FileAccess.Write, FileShare.None))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (string ip in BlackList)
                    {
                        writer.WriteLine($"0.0.0.0 {ip}");
                    }
                }
            }
        }

        public static void InstallCliTools()
        {
            throw new NotImplementedException();
        }

        private static void RunBlockingWithoutCli(string fileName, string arguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            proc.WaitForExit();
        }

        public static void ActivateWsl2()
        {
            // dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart

            RunBlockingWithoutCli("dism.exe", "/online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart");

            // dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart

            RunBlockingWithoutCli("dism.exe", "/online /enable-feature /featurename:VirtualMachinePlatform /all /norestart");

            // wsl --set-default-version 2

            RunBlockingWithoutCli("wsl.exe", "--set-default-version 2");
        }

        public static void RemoveTracking()
        {
            using (ServiceInstaller installer = new ServiceInstaller())
            {
                installer.Context = new InstallContext();
                installer.ServiceName = "DiagTrack";
                try { installer.Uninstall(null); } catch { }

                installer.Context = new InstallContext();
                installer.ServiceName = "dmwappushservice";
                try { installer.Uninstall(null); } catch { }
            }

            try { File.WriteAllText(@"C:\ProgramData\Microsoft\Diagnosis\ETLLogs\AutoLogger\AutoLogger-Diagtrack-Listener.etl", ""); } catch { }
        }

        public static void RemoveOneDrive()
        {
            // Closing OneDrive processes.
            
            foreach (var process in Process.GetProcessesByName("OneDrive.exe"))
            {
                try { process.Kill(true); } catch { }
            }

            // Uninstalling OneDrive.

            string sysDir = Environment.Is64BitOperatingSystem ? "SysWOW64" : "System32";
            string setupPath = Environment.ExpandEnvironmentVariables($"%SYSTEMROOT%\\{ sysDir }\\OneDriveSetup.exe");
            try { Process.Start(setupPath, "/uninstall"); } catch { }

            // Removing OneDrive leftovers.

            try { Directory.Delete(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\OneDrive"), true); } catch { }
            try { Directory.Delete(@"C:\OneDriveTemp", true); } catch { }
            try { Directory.Delete(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Microsoft\OneDrive"), true); } catch { }
            try { Directory.Delete(Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%\Microsoft OneDrive"), true); } catch { }

            // Removing OneDrive from the Explorer Side Panel.

            try { Registry.ClassesRoot.DeleteSubKey(@"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}"); } catch { }
            try { Registry.ClassesRoot.DeleteSubKey(@"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}"); } catch { }
        }

        public static void ActivateWindowsPhotoViewer()
        {
            RegistryKey key;

            key = Registry.ClassesRoot.CreateSubKey(@"Applications\photoviewer.dll\shell\open", true);
            if (key != null)
            {
                key.SetValue("AllowTelemetry", 0x0000000c, RegistryValueKind.DWord);
            }
        }
    }
}
