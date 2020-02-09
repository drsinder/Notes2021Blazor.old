using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2021Blazor.Shared
{
    public class LinkedFile
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int HomeFileId { get; set; }

        [Required]
        [StringLength(20)]
        public string HomeFileName { get; set; }

        [Required]
        [StringLength(20)]
        public string RemoteFileName { get; set; }

        [Required]
        [StringLength(450)]
        public string RemoteBaseUri { get; set; }

        [Required]
        public bool AcceptFrom { get; set; }

        [Required]
        public bool SendTo { get; set; }

        [StringLength(50)]
        public string Secret { get; set; }
    }
}
