using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Notes2021Blazor.Shared
{
    public class Subscription
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public int NoteFileId { get; set; }

        [Required]
        [StringLength(450)]
        public string SubscriberId { get; set; }

        [ForeignKey("NoteFileId")]
        public NoteFile NoteFile { get; set; }
    }
}
