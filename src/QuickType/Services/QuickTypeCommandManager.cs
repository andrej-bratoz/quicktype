using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QuickType.UI;
using QuickType.XML;

namespace QuickType.Services
{
    public class QuickTypeCommandManager
    {
        private static QuickTypeCommandManager _instance;

        public IntPtr WindowFocus { get; set; }


        public static QuickTypeCommandManager Instance => _instance ?? (_instance = new QuickTypeCommandManager());

        public List<QueryResult> ExecuteQuery(string query)
        {
            if (query == AppConstants.RELOAD_COMMAND)
            {
                CommandFactory.InitializeFromFile();
                return new List<QueryResult>();
            }


            var commands = CommandFactory.CommandCache.RegisteredCommands.Where(x => !string.IsNullOrEmpty(query) && x.Command.StartsWith(query)).OrderBy(x => x.Name).ToList();
            var keyShortcuts = commands.Where(x => x.Type == CommandType.KeyboardShortcut).ToList();
            var procShortcuts = commands.Where(x => x.Type == CommandType.ProcessOpen).ToList();

            var queryResult = CommandFactory.KeyboardShortcutsProvider(WindowFocus, keyShortcuts);
            queryResult.AddRange(CommandFactory.ProcessShortcutsCreator(procShortcuts, AppConstants.NO_FILTER));
            return queryResult;
        }

        public List<Tuple<string,string>> AutoKillWindowNames()
        {
            var commands = CommandFactory.CommandCache.RegisteredCommands.Where(x => x.Type == CommandType.AutoHandleWindow);
            return commands.Select(x => new Tuple<string,string>(x.Name,x.Keys)).ToList();
        }

        public List<QueryResult> GetContextCommands()
        {
            var text = Process.GetProcesses().Where(x => x.MainWindowHandle == WindowFocus).Select(x => x.ProcessName).FirstOrDefault();
#if DEBUG
            if(!string.IsNullOrEmpty(text)) Debug.WriteLine($"Process = {text}");
#endif
            if (string.IsNullOrEmpty(text))
            {
                if (WinApiProxy.IsExplorer(WindowFocus))
                {
                    text = AppConstants.EXPLORER_WINDOW;
                }
                else
                {
                    WindowFocus = WinApiProxy.GetParent(WindowFocus);
                    text = Process.GetProcesses().Where(x => x.MainWindowHandle == WindowFocus).Select(x => x.ProcessName).FirstOrDefault();
                }
                if (string.IsNullOrEmpty(text)) return new List<QueryResult>();
            }

            var vsCommands = CommandFactory.CommandCache.RegisteredCommands.Where(x => !string.IsNullOrEmpty(x.VisualStudioContext)).ToList();
            var commands = CommandFactory.CommandCache.RegisteredCommands.Where(x => x.WindowContext.StartsWith(text)).ToList();
            var otherctxCommands = CommandFactory.ProcessShortcutsCreator(vsCommands,text);
            var queryResult = CommandFactory.KeyboardShortcutsProvider(WindowFocus, commands);
            queryResult = queryResult.Where(x => otherctxCommands.All(y => y.Name != x.Name)).ToList();
            queryResult.AddRange(otherctxCommands);
            queryResult = queryResult.OrderBy(x => x.Name).ToList();
            return queryResult;
        }
    }
}
