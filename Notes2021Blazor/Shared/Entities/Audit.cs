using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2021Blazor.Shared
{
    public class Audit
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AuditID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        // Name of the user did made it
        [Required]
        [StringLength(256)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(450)]
        public string UserID { get; set; }

        [Required]
        [Display(Name = "Event Time")]
        public DateTime EventTime { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Event")]
        public string Event { get; set; }
    }
}
