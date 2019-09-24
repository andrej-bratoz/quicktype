using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using QuickType.Services;

namespace QuickType.XML
{
    public class SingleCommand
    {
        private string _visualStudioContext = "";
        private string _windowContext = "";
        private string _typeStr;

        [XmlAttribute("parameter")]
        public string Parameter { get; set; }

        [XmlAttribute("cmd")]
        public string Command { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("additional")]
        public string Additional { get; set; }

        [XmlAttribute("keys")]
        public string Keys { get; set; }

        [XmlAttribute("type")]
        public string TypeStr
        {
            get => _typeStr;
            set
            {
                value = ResolveVariable(value);
                _typeStr = value;
            }
        }

        public CommandType Type
        {
            get
            {
                if (int.TryParse(TypeStr, out int result))
                {
                    switch (result)
                    {
                        case 0:
                            return CommandType.KeyboardShortcut;
                        case 1:
                            return CommandType.ProcessOpen;
                        case 2:
                            return CommandType.AutoHandleWindow;
                    }
                }
                return CommandType.Unknown;
            }
        }

        [XmlAttribute("winctx")]
        public string WindowContext
        {
            get => _windowContext;
            set
            {
                value = ResolveVariable(value);
                _windowContext = value;
            }
        }

        [XmlAttribute("vsver")]
        public string VisualStudioContext
        {
            get => _visualStudioContext;
            set
            {
                value = ResolveVariable(value);
                _visualStudioContext = value;
            }
        }

        private static string ResolveVariable(string value)
        {
            var regMatch = (Regex.Match(value.ToUpperInvariant(), @"\$\(([_a-zA-Z0-9]+)\)"));
            if (regMatch.Success)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var key = (config.AppSettings.Settings.AllKeys.FirstOrDefault(x => x == regMatch.Groups[1].Value));
                if (key != null)
                {
                    value = config.AppSettings.Settings[key].Value;
                }
            }

            return value;
        }
    }
}