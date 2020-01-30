using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2021Blazor.Shared
{
    public class SQLFile
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FileId { get; set; }

        [Required]
        [StringLength(300)]
        public string FileName { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; }

        [Required]
        [StringLength(300)]
        public string Contributor { get; set; }

        public SQLFileContent Content { get; set; }


        [StringLength(1000)]
        public string Comments { get; set; }

    }

    public class SQLFileContent
    {
        [Key]
        public long SQLFileId { get; set; }

        public SQLFile SQLFile { get; set; }

        [Required]
        public byte[] Content { get; set; }
    }
}
