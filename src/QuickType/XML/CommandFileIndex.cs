using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QuickType.XML
{
    [XmlRoot("index")]
    public class CommandFileIndex
    {
        [XmlElement("cmdfile")]
        public List<IndexEntry> Entries { get; set; }
    }

    public class IndexEntry
    {
        [XmlAttribute("path")]
        public string FilePath { get; set; }
    }
}
