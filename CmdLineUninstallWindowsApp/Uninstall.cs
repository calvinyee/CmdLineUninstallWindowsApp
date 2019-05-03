using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdLineUninstallWindowsApp
{
    class Uninstall
    {
        public Uninstall ()
        {

        }

        // getUninstallString attempts to find the Uninstall string for a program (strProgramName)
        // in a given registry key area (key), using the specified Silent mode (bSilentMode)
        private string getUninstallString(RegistryKey key, string strProgramName, bool bSilentMode)
        {
            string displayName;
            string uninstallString;

            foreach (String keyName in key.GetSubKeyNames())
            {
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                if (strProgramName.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (bSilentMode)
                    {
                        uninstallString = subkey.GetValue("QuietUninstallString") as string;
                        if (uninstallString != null)  // in case there is no quiet unistall string, then fall thru to get non-quiet string
                            return uninstallString;
                        else
                            Console.WriteLine("QuietUninstallString is not available in the registry.  Silent mode might not work. ");
                    }
                    return subkey.GetValue("UninstallString") as string;
                }
            }
            return null;
        }

        // uninstallCommand parses the Uninstall string (uninstallCommand) using the silent mode (bSilentMode) and
        // actually executes the uninstall.
        private bool executeUninstall(string uninstallCommand, bool bSilentMode)
        {
            bool uninstalled;
            try
            {
                uninstallCommand = uninstallCommand.Replace("\"", ""); //Replace <">

                string uninstallArguments = null;
                string uninstallAssembly = null;
                if (!uninstallCommand.Contains("/"))
                { // parsing of somethin like: "rundll32.exe dfshim.dll,ShArpMaintain EncryptFiles.application, Culture=neutral, PublicKeyToken=aa2c539e4450c5b9, processorArchitecture=msil"
                    //uninstallAssembly = uninstallCommand;
                    int index = uninstallCommand.IndexOf(' ');
                    uninstallAssembly = uninstallCommand.Substring(0, index);
                    uninstallArguments = uninstallCommand.Substring(index + 1);
                }
                else //if file path has parameter or it is a relative path (for example msiexec.exe or cmd.exe)
                {  // parsing strings of something like: "\"C:\\Program Files (x86)\\Atlassian\\HipChat4\\unins000.exe\" /SILENT"
                    string[] uninstallArgumentsArray = uninstallCommand.Split(new string[] { " /" }, StringSplitOptions.RemoveEmptyEntries); // Split for any parameters
                    if (uninstallArgumentsArray.Count() > 1) // If 
                    {
                        for (int count = 1; count < uninstallArgumentsArray.Count(); count++)//building parameter string
                        {
                            uninstallArguments = "/" + uninstallArgumentsArray[count];
                        }
                    }
                    uninstallAssembly = uninstallArgumentsArray[0];// file name or path of the uninstall assembly are always on position 0 of the array
                }

                if (!string.IsNullOrWhiteSpace(uninstallAssembly)) //Do not execute process if no uninstall assembly path was found
                {
                    Process uninstallProcess = new Process();
                    uninstallProcess.StartInfo = new ProcessStartInfo();
                    uninstallProcess.StartInfo.FileName = uninstallAssembly;
                    if (bSilentMode && String.Equals(uninstallAssembly, "msiexec.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("SilentMode is still possible with msiexec.exe.");
                        uninstallArguments += " /qn";
                    }
                    uninstallProcess.StartInfo.Arguments = uninstallArguments;
                    uninstallProcess.Start();
                    uninstalled = true;
                }
                else
                {
                    uninstalled = false;//Not executed because no uninstall assembly path is avaible
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                uninstalled = false;//Unknown error occured
            }
            return uninstalled;//return if the kickoff for the uninstall process was successfull
        }

        // UninstallProgram searches the CurrentUser, LocalMachine_32, and LocalMachine_64 registry sections for the program to be 
        // uninstalled (ProgramName), with a given silent mode (bSilentMode).
        public bool UninstallProgram(string ProgramName, bool bSilentMode)
        {
            try
            {
                string strUninstall;
                RegistryKey key;
                bool uninstalled = false;

                key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if ((strUninstall = getUninstallString(key, ProgramName, bSilentMode)) != null)
                {  // search in: CurrentUser
                    Console.WriteLine("Searching for program in CurrentUser Registry.");
                    uninstalled = executeUninstall(strUninstall, bSilentMode);
                }
                else
                {
                    key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                    if ((strUninstall = getUninstallString(key, ProgramName, bSilentMode)) != null)
                    {   // search in: LocalMachine_32
                        Console.WriteLine("Searching for program in LocalMachine_32 Registry.");
                        uninstalled = executeUninstall(strUninstall, bSilentMode);
                    }
                    else
                    {
                        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                        if ((strUninstall = getUninstallString(key, ProgramName, bSilentMode)) != null)
                        {   // search in: LocalMachine_64
                            Console.WriteLine("Searching for program in LocalMachine_64 Registry.");
                            uninstalled = executeUninstall(strUninstall, bSilentMode);
                        }
                        /*
                        else
                        {  // this works, but slow and only works for msi installer programs.
                            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Product WHERE Name = '" + ProgramName + "'");

                            foreach (ManagementObject mo in mos.Get())
                            {
                                try
                                {
                                    string programeName = mo["Name"].ToString();
                                    Console.WriteLine(programeName);
                                    if (mo["Name"].ToString() == ProgramName)
                                    {
                                        object hr = mo.InvokeMethod("Uninstall", null);
                                        Console.WriteLine("rc = " + hr.ToString());
                                        return (bool)hr;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    uninstalled = false;
                                }
                            }

                        }
                        */
                    }
                }

                return uninstalled;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
