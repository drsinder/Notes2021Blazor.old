using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes2021Blazor.Shared
{
    public class Tags
    {
        // The fileid the note belongs to
        [Required]
        public int NoteFileId { get; set; }

        [Required]
        public int ArchiveId { get; set; }
        [Required]
        public long NoteHeaderId { get; set; }

        [ForeignKey("NoteHeaderId")]
        public NoteHeader NoteHeader { get; set; }

        [Required]
        [StringLength(30)]
        public string Tag { get; set; }

        public override string ToString()
        {
            return Tag;
        }

        public static string ListToString(List<Tags> list)
        {
            string s = string.Empty;
            if (list == null || list.Count < 1)
                return s;

            foreach (Tags tag in list)
            {
                s += tag.Tag + " ";
            }

            return s.TrimEnd(' ');
        }

        public static List<Tags> StringToList(string s)
        {
            List<Tags> list = new List<Tags>();

            if (string.IsNullOrEmpty(s) || s.Length < 1)
                return list;

            string[] tags = s.Split(',', ';', ' ');

            if (tags == null || tags.Length < 1)
                return list;

            foreach (string t in tags)
            {
                string r = t.Trim().ToLower();
                list.Add(new Tags() { Tag = r });
            }

            return list;
        }

        public static List<Tags> StringToList(string s, long hId, int fId, int arcId)
        {
            List<Tags> list = new List<Tags>();

            if (string.IsNullOrEmpty(s) || s.Length < 1)
                return list;

            string[] tags = s.Split(',', ';', ' ');

            if (tags == null || tags.Length < 1)
                return list;

            foreach (string t in tags)
            {
                string r = t.Trim().ToLower();
                list.Add(new Tags() { Tag = r, NoteHeaderId = hId, NoteFileId = fId, ArchiveId = arcId });
            }

            return list;
        }

        public static List<Tags> CloneForLink(List<Tags> inp)
        {
            if (inp == null)
                return null;

            List<Tags> outp = new List<Tags>();

            if (inp.Count == 0)
                return outp;

            foreach (Tags t in inp)
            {
                outp.Add(new Tags { Tag = t.Tag });
            }

            return outp;
        }
    }
}
