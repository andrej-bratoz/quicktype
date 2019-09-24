using System;
using System.IO;
using System.Linq;
using EnvDTE;
using QuickType.Exceptions;
using Process = System.Diagnostics.Process;

namespace QuickType.Services
{
    public class QuickTypeEvnVariables
    {
        public static string Resolve(string path, IntPtr contextwindow, string vsContext)
        {
            DTE visualStudioInstance = null;
            try
            {
                if (!path.Contains(AppConstants.VS_CURRENT_SOLUTION) &&
                    !path.Contains(AppConstants.VS_CURRENT_SOLUTION_DIR) &&
                    !path.Contains(AppConstants.VS_OPEN_FILE))
                {
                    return path;
                }

                var finalPath = path;
                var openVsCount = Process.GetProcesses().Count(x => x.ProcessName == AppConstants.VISUAL_STUDIO);
                if (openVsCount == 0)
                    throw new QuickTypeException(
                        "Cannot resolve Visual Studio Variables without any Visual Stdio instance running");

                var text = Process.GetProcesses().Where(x => x.MainWindowHandle == contextwindow)
                               .Select(x => x.ProcessName).FirstOrDefault() ?? "";
                if (text.ToLowerInvariant() != AppConstants.VISUAL_STUDIO && openVsCount > 1)
                {
                    throw new QuickTypeException("Cannot determine which Visual Studio is active");
                }

                // let's set context window to running VS instance
                if (text.ToLowerInvariant() != AppConstants.VISUAL_STUDIO)
                {
                    contextwindow = Process.GetProcesses().Where(x => x.MainWindowHandle == contextwindow)
                        .Select(x => x.MainWindowHandle).FirstOrDefault();
                }

                visualStudioInstance =
                    VSExplorer.GetDTE(vsContext, Process.GetProcesses().First(x => x.MainWindowHandle == contextwindow).Id);

                if (visualStudioInstance == null)
                {
                    throw new QuickTypeException("Cannot obtain handle to Visual Studio");
                }
                //
                if (path.Contains(AppConstants.VS_CURRENT_SOLUTION))
                {
                    finalPath = finalPath.Replace(AppConstants.VS_CURRENT_SOLUTION,
                        visualStudioInstance.Solution.FullName);
                }

                if (path.Contains(AppConstants.VS_CURRENT_SOLUTION_DIR))
                {
                    finalPath = finalPath.Replace(AppConstants.VS_CURRENT_SOLUTION_DIR,
                        Path.GetDirectoryName(visualStudioInstance.Solution.FullName));
                }

                if (path.Contains(AppConstants.VS_OPEN_FILE))
                {
                    var info = visualStudioInstance.Solution.DTE.Documents.DTE.ActiveDocument.FullName;
                    finalPath = finalPath.Replace(AppConstants.VS_OPEN_FILE, info);
                }

                //
                return finalPath;
            }
            // catch any Exception
            catch (Exception ex)
            {
                throw new QuickTypeException("Error while resolving QuickType variables",ex);
            }
            finally
            {
                VSExplorer.ReleaseVsInstance(visualStudioInstance);
            }
        }

    }
}