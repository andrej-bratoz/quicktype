using System;

namespace QuickType.Services
{
    public class QueryResult
    {
        public string Name { get; set; }
        public Action Data { get; set; }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}