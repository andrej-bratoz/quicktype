using System.Collections.Generic;
using System.Xml.Serialization;

namespace QuickType.XML
{
    [XmlRoot("commands")]
    public class CommandList
    {
        [XmlElement("command")]
        public List<SingleCommand> RegisteredCommands { get; } = new List<SingleCommand>();
    }
}
