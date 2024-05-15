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
        /// Check for another instance of this app and, if found, switch focus to it (restores if minimised).
        /// </summary>
        /// <returns>True if another instance of the app is already running.</returns>
        public static bool SwitchIfAlreadyRunning()
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
                            if (CTecUtil.UI.WindowUtil.IsIconic(otherInstance)) 
                                CTecUtil.UI.WindowUtil.ShowWindow(otherInstance, CTecUtil.UI.WindowUtil.SW_RESTORE);

                            //activate the other app
                            CTecUtil.UI.WindowUtil.SetForegroundWindow(otherInstance); 
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
