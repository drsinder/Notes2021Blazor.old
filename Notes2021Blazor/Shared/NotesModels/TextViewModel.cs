using System.ComponentModel.DataAnnotations;

namespace Notes2021Blazor.Shared
{
    /// <summary>
    /// Model used to input data for a note.
    /// </summary>
    public class TextViewModel
    {
        //[Required(ErrorMessage = "A Note body is required.")]
        //[StringLength(100000)]
        //[Display(Name = "MyNote")]
        public string MyNote { get; set; }

        //[Required(ErrorMessage = "A Subject is required.")]
        [StringLength(200)]
        [Display(Name = "MySubject")]
        public string MySubject { get; set; }

        //[Required]
        public int NoteFileID { get; set; }

        //[Required]
        public long BaseNoteHeaderID { get; set; }

        public long NoteID { get; set; }

        [StringLength(200)]
        [Display(Name = "Tags")]
        public string TagLine { get; set; }

        [StringLength(200)]
        [Display(Name = "Director Message")]
        public string DirectorMessage { get; set; }

        public NoteHeader NoteHeader { get; set; }

    }

    public class SCheckModel
    {
        public bool isChecked { get; set; }
        public int fileId { get; set; }
    }
}


