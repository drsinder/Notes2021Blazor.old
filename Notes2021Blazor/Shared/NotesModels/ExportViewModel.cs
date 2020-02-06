
namespace Notes2021Blazor.Shared
{
    public class ExportViewModel
    {
        public NoteFile NoteFile { get; set; }

        public int ArchiveNumber { get; set; }

        public bool isHtml { get; set; }

        public bool isCollapsible { get; set; }

        public bool isDirectOutput { get; set; }

        public int NoteOrdinal { get; set; }

        public string Email { get; set; }

    }

}
