using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Notes2021Blazor.Shared
{
    public class TZone
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // ReSharper disable once InconsistentNaming
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Abbreviation { get; set; }

        [Required]
        public string Offset { get; set; }

        [Required]
        public int OffsetHours { get; set; }

        [Required]
        public int OffsetMinutes { get; set; }

        public DateTime Local(DateTime dt)
        {
            return dt.AddHours(OffsetHours).AddMinutes(OffsetMinutes);
        }

        public DateTime Universal(DateTime dt)
        {
            return dt.AddHours(-OffsetHours).AddMinutes(-OffsetMinutes);
        }
    }

}
