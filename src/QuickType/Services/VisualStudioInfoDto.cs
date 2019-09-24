using EnvDTE;

namespace QuickType.Services
{
    public class VisualStudioInfoDto
    {
        public string Version { get; set; }
        public int ProcessId { get; set; }
        public DTE VsInstance { get; set; }
    }
}