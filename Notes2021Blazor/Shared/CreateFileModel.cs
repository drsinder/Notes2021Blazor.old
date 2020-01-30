using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
