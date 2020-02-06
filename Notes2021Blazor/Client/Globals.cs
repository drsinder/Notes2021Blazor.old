
namespace Notes2021Blazor.Client
{
    public class Globals
    {
        public static string SendGridEmail { get; set; } = "sinder@illinois.edu";

        public static string SendGridName { get; set; } = "Dale Sinder";

        public static string SendGridApiKey { get; set; }

        public static string EmailName { get; set; } = "Dale Sinder";

        public static string AccessOther() { return "Other"; }

        public static string AccessOtherId() { return "Other"; }

        public static string ImportedAuthorId() { return "*imported*"; }

    }
}
