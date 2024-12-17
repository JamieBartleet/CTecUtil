using CTecUtil.UI.Util;
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
        /// Check for another instance of this app.
        /// </summary>
        /// <returns>True if another instance of the app is already running.</returns>
        public static bool CheckForAnotherInstance()
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();

                //check whether another instance is already running
                foreach (var p in Process.GetProcesses())
                    if (p.Id != currentProcess.Id)
                        if (p.ProcessName.Equals(currentProcess.ProcessName)
                         && (DateTime?)p.StartTime is not null)                 //<-- NB: under Win11 TWO processes are started on startup, the 'real' one has proper data attached
                            return true;
            }
            catch { }
            return false;
        }

    
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

                //check whether another instance is already running
                foreach (var p in Process.GetProcesses())
                {
                    if (p.Id != currentProcess.Id)
                    {
                        if (p.ProcessName.Equals(currentProcess.ProcessName))
                        {
                            isAlreadyRunning = true;
                            IntPtr otherInstance = p.MainWindowHandle;

                            //if other app is minimised retore it
                            if (WindowUtil.IsIconic(otherInstance)) 
                                WindowUtil.ShowWindow(otherInstance, WindowUtil.SW_RESTORE);

                            //activate the other app
                            WindowUtil.SetForegroundWindow(otherInstance); 
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
