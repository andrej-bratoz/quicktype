using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using QuickType.Exceptions;
using QuickType.UI;
using QuickType.XML;
using ConfigurationManager = EnvDTE.ConfigurationManager;
using Process = System.Diagnostics.Process;

namespace QuickType.Services
{
    public static class CommandFactory
    {
        private static string _commandFile;

        public static string CommandFile
        {
            get
            {
                if (_commandFile == null)
                {
                    System.Configuration.Configuration config =
                        System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    _commandFile = config.AppSettings.Settings[AppConstants.INDEX_CMD_FILE].Value;
                }
                return _commandFile;
            }
        }

        public static CommandList CommandCache { get; set; }
        public static void InitializeFromFile()
        {
            CommandCache = new CommandList();
            var serializer = new XmlSerializer(typeof(CommandFileIndex));
            using (var fileReader = File.OpenRead(CommandFile))
            {
                var cmdFileDir = Path.GetDirectoryName(Path.GetFullPath(CommandFile));
                if(string.IsNullOrEmpty(cmdFileDir)) throw new QuickTypeException("Unable to read command file index");
                var cmdfile = (CommandFileIndex) serializer.Deserialize(fileReader);
                cmdfile.Entries.ForEach(x =>
                {
                    var cmdSerializer = new XmlSerializer(typeof(CommandList));
                    var combinedPath = Path.Combine(cmdFileDir, x.FilePath);
                    using (var fReader = File.OpenRead(combinedPath))
                    {
                        if (!File.Exists(combinedPath)) return;
                        var cache = (CommandList)cmdSerializer.Deserialize(fReader);
                        CommandCache.RegisteredCommands.AddRange(cache.RegisteredCommands);
                    }
                });
            }
        }

        public static List<QueryResult> ProcessShortcutsCreator(List<SingleCommand> commands, string filter)
        {
            var cmd = commands;
            if (!string.IsNullOrEmpty(filter))
            {
                cmd = cmd.Where(x => x.WindowContext == filter).ToList();
            }
            return cmd.Select(x => new QueryResult()
            {
                Data = () =>
                {
                    try
                    {
                        var expanded = (Environment.ExpandEnvironmentVariables(x.Parameter));
                        if (!string.IsNullOrEmpty(x.Additional))
                        {
                            var args = QuickTypeEvnVariables.Resolve(x.Additional,
                                QuickTypeCommandManager.Instance.WindowFocus,
                                x.VisualStudioContext);

                            var process = Process.Start($"{expanded}",
                                $"{args}");
                            if (process != null) WinApiProxy.SetForegroundWindow(process.Handle);
                        }
                        else
                        {
                            var process = Process.Start($"{expanded}");
                            if (process != null) WinApiProxy.SetForegroundWindow(process.Handle);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}\n{ex?.InnerException?.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                },
                Name = x.Name
            }).OrderBy(x => x.Name).ToList();
        }

        public static List<QueryResult> KeyboardShortcutsProvider(IntPtr window, List<SingleCommand> cmd)
        {
            return cmd.Select(x => new QueryResult()
            {
                Data = () =>
                {
                    WinApiProxy.SetForegroundWindow(window);
                    SendKeys.SendWait(x.Keys);
                },
                Name = x.Name
            }).OrderBy(x => x.Name).ToList();
        }

    }
}