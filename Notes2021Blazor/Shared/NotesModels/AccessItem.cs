namespace Notes2021Blazor.Shared
{
    public enum AccessX
    {
        ReadAccess,
        Respond,
        Write,
        SetTag,
        DeleteEdit,
        ViewAccess,
        EditAccess
    }
    public class AccessItem
    {
        public NoteAccess Item { get; set; }
        public AccessX which { get; set; }
        public bool isChecked { get; set; }
        public bool canEdit { get; set; }
    }

}
