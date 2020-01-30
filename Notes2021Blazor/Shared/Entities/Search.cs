using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2021Blazor.Shared
{
    public enum SearchOption { Author, Title, Content, Tag, DirMess, TimeIsAfter, TimeIsBefore }

    /// <summary>
    /// Model for searching a notefile
    /// </summary>
    public class Search
    {
        // User doing the search
        [StringLength(450)]
        public string UserId { get; set; }

        // search specs Option
        [Display(Name = "Search By")]
        public SearchOption Option { get; set; }

        // Text to search for
        [Display(Name = "Search Text")]
        public string Text { get; set; }

        // DateTime to compare to
        [Display(Name = "Search Date/Time")]
        public DateTime Time { get; set; }

        // current/next info -- where we are in the search
        [Column(Order = 0)]
        public int NoteFileId { get; set; }

        [Required]
        [Column(Order = 1)]
        public int ArchiveId { get; set; }

        [Column(Order = 2)]
        public int BaseOrdinal { get; set; }
        [Column(Order = 3)]
        public int ResponseOrdinal { get; set; }
        [Column(Order = 4)]
        public long NoteID { get; set; }

        [ForeignKey("NoteFileId")]
        public NoteFile NoteFile { get; set; }

        // Makes a clone of the object.  Had to do this to avoid side effects.
        public Search Clone(Search s)
        {
            Search cloned = new Search
            {
                BaseOrdinal = s.BaseOrdinal,
                NoteFileId = s.NoteFileId,
                NoteID = s.NoteID,
                Option = s.Option,
                ResponseOrdinal = s.ResponseOrdinal,
                Text = s.Text,
                Time = s.Time,
                UserId = s.UserId
            };
            return cloned;
        }

    }
}
