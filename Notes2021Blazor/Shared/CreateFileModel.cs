using System.ComponentModel.DataAnnotations;

namespace Notes2021Blazor.Shared
{
    public class CreateFileModel
    {
        [Required]
        public string NoteFileName { get; set; }
        [Required]
        public string NoteFileTitle { get; set; }

    }

}
