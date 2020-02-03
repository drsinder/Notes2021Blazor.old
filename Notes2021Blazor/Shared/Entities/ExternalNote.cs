using System.ComponentModel.DataAnnotations;

namespace Notes2021Blazor.Shared
{
    public class ExternalNote
    {
        [StringLength(100)]
        [Key]
        public string NoteGuid { get; set; }

        public int FileId { get; set; }

        public int ArchiveId { get; set; }

        public long BaseNoteId { get; set; }

        public long EditNoteId { get; set; }

        [StringLength(200)]
        public string Heading { get; set; }

        [StringLength(200)]
        [Display(Name = "Subject")]
        public string NoteSubject { get; set; }

        [StringLength(200)]
        [Display(Name = "Director Message")]
        public string DirectorMessage { get; set; }

        [StringLength(200)]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        [StringLength(100000)]
        [Display(Name = "Note")]
        public string NoteBody { get; set; }

    }
}
