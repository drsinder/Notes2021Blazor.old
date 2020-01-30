using Hangfire;
using Notes2021Lib.Data;
using PusherServer;
using System;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server
{
    public class Jobs
    {
        public async Task UpdateHomePageTime(string username, TZone tzone)
        {
            string stuff = tzone.Local(DateTime.Now.ToUniversalTime()).ToShortTimeString() + " " + tzone.Abbreviation + " - " +
                tzone.Local(DateTime.Now.ToUniversalTime()).ToLongDateString();

            var options = new PusherOptions()
            {
                Encrypted = true,
                Cluster = Globals.PusherCluster
            };
            Pusher pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);
            var data = new { message = stuff };
            ITriggerResult x = await pusher.TriggerAsync("notes-data-" + username, "update-time", data);
            if (x.StatusCode != System.Net.HttpStatusCode.OK)
                RecurringJob.RemoveIfExists(username);
        }

#pragma warning disable 1998
        public async Task CleanupHomePageTime(string username)
#pragma warning restore 1998
        {
            RecurringJob.RemoveIfExists(username);
            RecurringJob.RemoveIfExists("delete_" + username);
        }
    }

}
