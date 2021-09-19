using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Net;
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

            Dictionary<string, string> entries = new Dictionary<string, string>();

            // read and store existing entries
            using (FileStream stream = new FileStream(@"C:\Windows\System32\drivers\etc\hosts", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length != 0 && line[0] != '#') // don't try to parse comments, we'll just erase them
                    {
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (line[i] == ' ' || line[i] == '\t')
                            {
                                // we have found a delimeter, thus we can now separate the "key" and the "not key" part of the line
                                entries[line.Substring(i + 1)] = line.Substring(0, i);
                                // we are no longer interested in looking for delimeters
                                break;
                            }
                        }
                    }
                }
            }

            // we're putting it here, to make sure that none of the blacklisted hosts are getting whitelisted by the file's current entries
            foreach (string ip in BlackList)
            {
                entries[ip] = "0.0.0.0";
            }

            // write out all entries
            using (FileStream stream = new FileStream(@"C:\Windows\System32\drivers\etc\hosts", FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                foreach (KeyValuePair<string, string> entry in entries)
                {
                    writer.WriteLine($"{entry.Value} {entry.Key}");
                }
            }

            // flush dns cache
            RunBlockingWithoutCli("ipconfig", "/flushdns");
        }

        private static void InstallLatestRelease(string owner, string repo)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://api.github.com/repos/{ owner }/{ repo }/releases/latest");
            request.ContentType = "application/vnd.github.v3+json";
            request.UserAgent = "WinTy";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using Stream responseStream = response.GetResponseStream();
            using StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);

            string responseString = streamReader.ReadToEnd();
            JObject root = JObject.Parse(responseString);
            JArray assets = (JArray)root["assets"];
            JObject msixbundle = null;
            foreach (JObject asset in assets)
            {
                string name = (string)asset["name"];
                if (name.EndsWith(".msixbundle"))
                {
                    msixbundle = asset;
                    break;
                }
            }

            WebClient client = new WebClient();
            client.Headers.Add("User-Agent", "WinTy");
            string remoteUrl = (string)(msixbundle["url"]);
            string localUrl = Path.GetTempFileName();

            client.DownloadFile(remoteUrl, localUrl);
            Debug.WriteLine(localUrl);

            PowerShell.Create().AddCommand("Set-ExecutionPolicy").AddParameter("Scope", "CurrentUser").AddParameter("ExecutionPolicy", "Unrestricted").Invoke();
            PowerShell.Create().AddCommand("Import-Module").AddArgument("Appx").Invoke();
            PowerShell.Create().AddCommand("Add-AppxPackage").AddParameter("Path", localUrl).Invoke();

            File.Delete(localUrl);
        }

        public static void InstallCliTools()
        {
            InstallLatestRelease("microsoft", "winget-cli");
            InstallLatestRelease("microsoft", "terminal");
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
            // closing OneDrive processes
            
            foreach (var process in Process.GetProcessesByName("OneDrive.exe"))
            {
                try { process.Kill(true); } catch { }
            }

            // uninstalling OneDrive

            string sysDir = Environment.Is64BitOperatingSystem ? "SysWOW64" : "System32";
            string setupPath = Environment.ExpandEnvironmentVariables($"%SYSTEMROOT%\\{ sysDir }\\OneDriveSetup.exe");
            try { Process.Start(setupPath, "/uninstall"); } catch { }

            // removing OneDrive leftovers

            try { Directory.Delete(Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\OneDrive"), true); } catch { }
            try { Directory.Delete(@"C:\OneDriveTemp", true); } catch { }
            try { Directory.Delete(Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Microsoft\OneDrive"), true); } catch { }
            try { Directory.Delete(Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%\Microsoft OneDrive"), true); } catch { }

            // removing OneDrive from the Explorer Side Panel

            try { Registry.ClassesRoot.DeleteSubKey(@"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}"); } catch { }
            try { Registry.ClassesRoot.DeleteSubKey(@"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}"); } catch { }
        }

        public static void ActivateWindowsPhotoViewer()
        {
            byte[] command = new byte[] { 37, 0, 83, 0, 121, 0, 115, 0, 116, 0, 101, 0, 109, 0, 82, 0, 111, 0, 111, 0, 116, 0, 37, 0, 92, 0, 83, 0, 121, 0, 115, 0, 116, 0, 101, 0, 109, 0, 51, 0, 50, 0, 92, 0, 114, 0, 117, 0, 110, 0, 100, 0, 108, 0, 108, 0, 51, 0, 50, 0, 46, 0, 101, 0, 120, 0, 101, 0, 32, 0, 34, 0, 37, 0, 80, 0, 114, 0, 111, 0, 103, 0, 114, 0, 97, 0, 109, 0, 70, 0, 105, 0, 108, 0, 101, 0, 115, 0, 37, 0, 92, 0, 87, 0, 105, 0, 110, 0, 100, 0, 111, 0, 119, 0, 115, 0, 32, 0, 80, 0, 104, 0, 111, 0, 116, 0, 111, 0, 32, 0, 86, 0, 105, 0, 101, 0, 119, 0, 101, 0, 114, 0, 92, 0, 80, 0, 104, 0, 111, 0, 116, 0, 111, 0, 86, 0, 105, 0, 101, 0, 119, 0, 101, 0, 114, 0, 46, 0, 100, 0, 108, 0, 108, 0, 34, 0, 44, 0, 32, 0, 73, 0, 109, 0, 97, 0, 103, 0, 101, 0, 86, 0, 105, 0, 101, 0, 119, 0, 95, 0, 70, 0, 117, 0, 108, 0, 108, 0, 115, 0, 99, 0, 114, 0, 101, 0, 101, 0, 110, 0, 32, 0, 37, 0, 49, 0, 0, 0 };

            RegistryKey key;

            key = Registry.ClassesRoot.CreateSubKey(@"Applications\photoviewer.dll\shell\open", true);
            if (key != null)
            {
                key.SetValue("MuiVerb", "@photoviewer.dll,-3043", RegistryValueKind.String);
            }

            key = Registry.ClassesRoot.CreateSubKey(@"Applications\photoviewer.dll\shell\open\command", true);
            if (key != null)
            {
                key.SetValue("MuiVerb", command, RegistryValueKind.Binary);
            }

            key = Registry.ClassesRoot.CreateSubKey(@"Applications\photoviewer.dll\shell\open\DropTarget", true);
            if (key != null)
            {
                key.SetValue("Clsid", "{FFE2A43C-56B9-4bf5-9A79-CC6D4285608A}", RegistryValueKind.String);
            }

            key = Registry.ClassesRoot.CreateSubKey(@"Applications\photoviewer.dll\shell\print\command", true);
            if (key != null)
            {
                key.SetValue("MuiVerb", command, RegistryValueKind.Binary);
            }

            key = Registry.ClassesRoot.CreateSubKey(@"Applications\photoviewer.dll\shell\print\DropTarget", true);
            if (key != null)
            {
                key.SetValue("Clsid", "{60fd46de-f830-4894-a628-6fa81bc0190d}", RegistryValueKind.String);
            }
        }
    }
}
