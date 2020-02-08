using System.ComponentModel.DataAnnotations;


namespace Notes2021Blazor.Shared
{
    public class NoteContent
    {
        [Required]
        [Key]
        public long NoteHeaderId { get; set; }

        //[ForeignKey("NoteHeaderId")]
        public NoteHeader NoteHeader { get; set; }

        // The Body or content of the note
        [Required]
        [StringLength(100000)]
        [Display(Name = "Note")]
        public string NoteBody { get; set; }

        // for imported notes compatability
        [StringLength(200)]
        [Display(Name = "Director Message")]
        public string DirectorMessage { get; set; }

        public NoteContent CloneForLink()
        {
            NoteContent nc = new NoteContent()
            {
                NoteBody = NoteBody,
                DirectorMessage = DirectorMessage
            };

            return nc;
        }
    }
}
