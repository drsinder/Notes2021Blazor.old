using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2021Blazor.Shared
{
    public class Sequencer
    {
        // ID of the user who owns the item
        [Required]
        [Column(Order = 0)]
        [StringLength(450)]
        public string UserId { get; set; }

        // ID of target notfile
        [Required]
        [Column(Order = 1)]
        public int NoteFileId { get; set; }

        [Required]
        [Display(Name = "Position in List")]
        public int Ordinal { get; set; }

        // Time we last completed a run with this
        [Display(Name = "Last Time")]
        public DateTime LastTime { get; set; }

        // Time a run in this file started - will get copied to LastTime when complete
        public DateTime StartTime { get; set; }

        // Is this item active now?  Are we sequencing this file
        public bool Active { get; set; }

        [ForeignKey("NoteFileId")]
        public NoteFile NoteFile { get; set; }
    }
}
