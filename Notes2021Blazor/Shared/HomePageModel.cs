using System;
using System.Collections.Generic;
using System.Text;

namespace Notes2021Blazor.Shared
{
    public class HomePageModel
    {
        public List<NoteFile> NoteFiles { get; set; }

        public TZone TimeZone { get; set; }

        public HomePageMessage Message { get; set; }

        public UserData UserData { get; set; }
    }
}
