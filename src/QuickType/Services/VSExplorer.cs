using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace QuickType.Services
{
    // https://blogs.msdn.microsoft.com/kirillosenkov/2011/08/10/how-to-get-dte-from-visual-studio-process-id/
    public class VSExplorer
    {
       

        /// <summary>
        /// Get a COM handle to DTE of a running VS instance
        /// </summary>
        /// <param name="vsVersion">Version of Visual Studio (15.0 for 2017)</param>
        /// <param name="processId">Process ID of the running version</param>
        /// <returns></returns>
        public static DTE GetDTE(string vsVersion, int processId)
        {
            //var vsRunningInstance = BoundInstances.FirstOrDefault(x => x.ProcessId == processId && x.Version == vsVersion);
            //if (vsRunningInstance != null)
            //{
            //    return vsRunningInstance.VsInstance;
            //}

            string progId = $"!VisualStudio.DTE.{vsVersion}:{processId.ToString()}";
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(WinApiProxy.CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                IMoniker[] moniker = new IMoniker[1];
                IntPtr numberFetched = IntPtr.Zero;
                while (enumMonikers.Next(1, moniker, numberFetched) == 0)
                {
                    IMoniker runningObjectMoniker = moniker[0];

                    string name = null;

                    try
                    {
                        runningObjectMoniker?.GetDisplayName(bindCtx, null, out name);
                        Debug.WriteLine($"Name = {name}" );
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }
                    if (!string.IsNullOrEmpty(name) && string.Equals(name, progId, StringComparison.Ordinal))
                    {
                        Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }
            return (DTE)runningObject;
        }

        public static void ReleaseVsInstance(DTE dte)
        {
            if (dte != null)
            {
                Marshal.ReleaseComObject(dte);
            }
        }
    }
}
