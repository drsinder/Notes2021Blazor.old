using System.Collections.Generic;

namespace Notes2021Blazor.Shared
{
    public class DisplayModel
    {
        public NoteContent content { get; set; }
        public List<Tags> tags { get; set; }
    }
}
