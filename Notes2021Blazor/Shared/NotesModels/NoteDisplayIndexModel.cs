using System.Collections.Generic;

namespace Notes2021Blazor.Shared
{
    public class NoteDisplayIndexModel
    {
        public NoteFile noteFile { get; set; }
        public int ArcId { get; set; }
        public NoteAccess myAccess { get; set; }
        public bool isMarked { get; set; }
        public string rPath { get; set; }
        public string scroller { get; set; }
        public int ExpandOrdinal { get; set; }
        public List<NoteHeader> Notes { get; set; }
        public NoteHeader myHeader { get; set; }
        public TZone tZone { get; set; }
        public List<NoteHeader> AllNotes { get; set; }
        public string linkedText { get; set; }
        public string message { get; set; }
    }

}
