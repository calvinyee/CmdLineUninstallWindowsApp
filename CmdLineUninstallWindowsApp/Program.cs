using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;
using System.Diagnostics;

// This is a  C# command line app that can uninstall applications from Windows.
//  Usage: CmdLineUninstallWindowsApp ProgamName -Silent
//  The Silent parameter is optional and not case sensitive, if no provided, uninstall will show the normal uninstall UI
// Example 1 : CmdLineUninstallWindowsApp MyApp
// Example 2:  CmdLineUninstallWindowsApp \"MyApp with Spaces\""
// Example 3:  CmdLineUninstallWindowsApp MyApp -Silent"
// Example 4:  CmdLineUninstallWindowsApp \"MyApp with Spaces\" -Silent"
//
// Note: In order for the silent install to work,  the app.manifest file requestedExecutionLevel has be raised to "requireAdministrator" so the user is prompted for admin
// privileges when this program is initally run.

namespace CmdLineUninstallWindowsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Parameters missing, you need to provide the program name and an optional Silent parameter");
                Console.WriteLine("Syntax:   CmdLineUninstallWindowsApp ProgamName -Silent");
                Console.WriteLine("Example 1:  CmdLineUninstallWindowsApp MyApp");
                Console.WriteLine("Example 2:  CmdLineUninstallWindowsApp \"MyApp with Spaces\"");
                Console.WriteLine("Example 3:  CmdLineUninstallWindowsApp MyApp -Silent");
                Console.WriteLine("Example 4:  CmdLineUninstallWindowsApp \"MyApp with Spaces\" -Silent");
            }
            else
            {
                string strProgramName = args[0];
                bool bSilentMode = args.Length > 1 && String.Equals(args[1], "-silent", StringComparison.OrdinalIgnoreCase);
                Uninstall uninstall = new Uninstall();
                if (uninstall.UninstallProgram(strProgramName, bSilentMode))
                {
                    if (bSilentMode)
                        Console.WriteLine("Uninstall of " + strProgramName + " might involve user interaction to be successful. ");
                    Console.WriteLine("Uninstall of " + strProgramName + " was successful. ");
                }
                else
                    Console.WriteLine("Uninstall of " + strProgramName + " failed.");
            }
            Console.ReadKey();
        }
    }
}
