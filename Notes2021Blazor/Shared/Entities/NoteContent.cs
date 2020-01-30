using System.ComponentModel.DataAnnotations;


namespace Notes2021Blazor.Shared
{
    public class NoteContent
    {
        [Required]
        [Key]
        public long NoteHeaderId { get; set; }

        //[ForeignKey("NoteHeaderId")]
        public NoteHeader NoteHeader { get; set; }

        // The Body or content of the note
        [Required]
        [StringLength(100000)]
        [Display(Name = "Note")]
        public string NoteBody { get; set; }

        // for imported notes compatability
        [StringLength(200)]
        [Display(Name = "Director Message")]
        public string DirectorMessage { get; set; }

        public NoteContent CloneForLink()
        {
            NoteContent nc = new NoteContent()
            {
                NoteBody = NoteBody,
                DirectorMessage = DirectorMessage
            };

            return nc;
        }

        //#region IDisposable Support
        //private bool disposedValue = false; // To detect redundant calls
        //SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //            return;

        //        if (disposing)
        //        {
        //            handle.Dispose();
        //            // Free any other managed objects here.
        //            //
        //        }

        //        disposedValue = true;
        //    }
        //}

        //// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //~NoteContent()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(false);
        //}

        //// This code added to correctly implement the disposable pattern.
        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}
        //#endregion
    }
}
