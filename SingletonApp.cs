using CTecUtil.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil
{
    public class SingletonApp
    {
        /// <summary>
        /// Checks whether any instance of this app is already running.  If so, switches focus to that one.
        /// </summary>
        /// <returns>True if another instance of the app is already running.</returns>
        public static bool IsRunning()
        {
            bool isAlreadyRunning = false;
            try
            {
                Process currentProcess = Process.GetCurrentProcess();

                //check wheter another instance is already running
                foreach (var p in Process.GetProcesses())
                {
                    if (p.Id != currentProcess.Id)
                    {
                        if (p.ProcessName.Equals(currentProcess.ProcessName))
                        {
                            isAlreadyRunning = true;
                            IntPtr otherInstance = p.MainWindowHandle;

                            //if other app is minimised retore it
                            if (User32API.IsIconic(otherInstance)) 
                                User32API.ShowWindow(otherInstance, User32API.SW_RESTORE);

                            //activate the other app
                            User32API.SetForegroundWindow(otherInstance); 
                            break;
                        }
                    }
                }
            }
            catch { }
            return isAlreadyRunning;
        }
    }
}
