using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2021Blazor.Shared
{
    public class NoteAccess
    {
        [Required]
        [Column(Order = 0)]
        [StringLength(450)]
        public string UserID { get; set; }

        [Required]
        [Column(Order = 1)]
        public int NoteFileId { get; set; }

        [Required]
        [Column(Order = 2)]
        public int ArchiveId { get; set; }

        [ForeignKey("NoteFileId")]
        public NoteFile NoteFile { get; set; }

        // Control options

        [Required]
        [Display(Name = "Read")]
        public bool ReadAccess { get; set; }

        [Required]
        [Display(Name = "Respond")]
        public bool Respond { get; set; }

        [Required]
        [Display(Name = "Write")]
        public bool Write { get; set; }

        [Required]
        [Display(Name = "Set Tag")]
        public bool SetTag { get; set; }

        [Required]
        [Display(Name = "Delete/Edit")]
        public bool DeleteEdit { get; set; }

        [Required]
        [Display(Name = "View Access")]
        public bool ViewAccess { get; set; }

        [Required]
        [Display(Name = "Edit Access")]
        public bool EditAccess { get; set; }
    }
}
