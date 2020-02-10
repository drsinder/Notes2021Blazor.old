using Notes2021Blazor.Shared;
using System.Collections.Generic;

namespace Notes2021.Models
{
    public class NoteDisplayIndexModel
    {
        public NoteFile noteFile { get; set; }

        //public List<NoteHeader> NoteHeaders { get; set; }

        public int ArcId { get; set; }
        public NoteAccess myAccess { get; set; }
        public bool isMarked { get; set; }
        public string rPath { get; set; }
        public string scroller { get; set; }
        public int ExpandOrdinal { get; set; }
        public List<NoteHeader> Notes { get; set; }
        public List<string> Cheaders { get; set; }
        public List<string> Lheaders { get; set; }
        public string panelEnd { get; set; }
        public TZone tZone { get; set; }
        public List<NoteHeader> Expanded { get; set; }
        public string linkedText { get; set; }
    }
}
