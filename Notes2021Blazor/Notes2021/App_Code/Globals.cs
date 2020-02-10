using Microsoft.AspNetCore.Identity.UI.Services;
using System;

namespace Notes2021
{
    public class Globals
    {
        public static DateTime StartupDateTime { get; set; }

        public static TimeSpan Uptime()
        {
            return DateTime.Now.ToUniversalTime() - StartupDateTime;

        }

        public static string PathBase { get; set; }

        //public static string AdminEmail { get; set; }

        public static string SendGridEmail { get; set; }

        public static string SendGridName { get; set; }

        public static string SendGridApiKey { get; set; }

        public static string EmailName { get; set; }

        public static string AccessOther() { return "Other"; }

        public static string AccessOtherId() { return "Other"; }

        public static string ImportedAuthorId() { return "*imported*"; }

        public static string ProductionUrl { get; set; }

        public static int TimeZoneDefaultID { get; set; }

        public static string PusherAppId { get; set; }
        public static string PusherKey { get; set; }
        public static string PusherSecret { get; set; }
        public static string PusherCluster { get; set; }

        public static string ChatKitAppLoc { get; set; }
        public static string ChatKitKey { get; set; }

        public static IEmailSender EmailSender { get; set; }

        public static string DBConnectString { get; set; }

        public static string PrimeAdminName { get; set; }
        public static string PrimeAdminEmail { get; set; }
    }
}
