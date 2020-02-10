using Notes2021Blazor.Shared;

namespace Notes2021.Models
{
    public class NoteXModel
    {
        public NoteHeader nh { get; set; }
        public NoteHeader bnh { get; set; }
        public NoteAccess myAccess { get; set; }
        public bool CanDelete { get; set; }
        public bool IsSeq { get; set; }
        public string DeleteMessage { get; set; }
        public TZone tZone { get; set; }

    }
}
