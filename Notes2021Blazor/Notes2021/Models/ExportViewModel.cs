using Microsoft.AspNetCore.Mvc.Rendering;
using Notes2021Blazor.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Notes2021.Models
{
    /// <summary>
    /// Model used for Exporting files
    /// </summary>
    public class ExportViewModel
    {
        [Required]
        [StringLength(20)]
        public string FileName { get; set; }

        public int FileNum { get; set; }

        public List<SelectListItem> AFiles { get; set; }

        public List<SelectListItem> ATitles { get; set; }

        [Display(Name = "As Html - otherwise plain text.  Plain text can be reimported.")]
        public bool isHtml { get; set; }

        [Display(Name = "Collapsible/Expandable")]
        public bool isCollapsible { get; set; }

        [Display(Name = "Direct Output to Browser")]
        public bool directOutput { get; set; }

        public int NoteOrdinal { get; set; }

        // ReSharper disable once UnusedMember.Global
        public TZone tzone { get; set; }

        // ReSharper disable once UnusedMember.Global
        public string Message { get; set; }
    }

}
