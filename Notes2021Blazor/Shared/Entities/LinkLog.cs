using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Notes2021Blazor.Shared
{
    public class LinkLog
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        [Required]
        [Display(Name = "Event Time")]
        public DateTime EventTime { get; set; }

        [Required]
        [Display(Name = "Event")]
        public string Event { get; set; }
    }
}
