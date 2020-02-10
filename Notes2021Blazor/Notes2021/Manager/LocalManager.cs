using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notes2021.Controllers;
using Notes2021Blazor.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Notes2021.Manager
{
    public static class LocalManager
    {
        public static UserData GetUserData(UserManager<IdentityUser> userManager, ClaimsPrincipal user, NotesDbContext db)
        {
            UserData aux = null;
            try
            {
                string userid = userManager.GetUserId(user);
                aux = db.UserData.SingleOrDefault(p => p.UserId == userid);
            }
            catch
            { }
            return aux;
        }

        public static async Task<TZone> GetUserTimeZone(HttpContext httpContext,
    ClaimsPrincipal userx, UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager, NotesDbContext db)
        {

            TZone tz = SessionExtensionsLocal.GetObject<TZone>(httpContext.Session, "TZone");
            if (tz != null)
                return SessionExtensionsLocal.GetObject<TZone>(httpContext.Session, "TZone");

            int tzid = Globals.TimeZoneDefaultID;
            if (signInManager.IsSignedIn(userx))
            {
                try
                {
                    string userId = userManager.GetUserId(userx);
                    tzid = GetUserData(userManager, userx, db).TimeZoneID;  // get users timezoneid
                }
                catch
                {
                    // ignored
                }
            }
            if (tzid < 1)
                tzid = Globals.TimeZoneDefaultID;

            var tz2 = await db.TZone.SingleAsync(p => p.Id == tzid);
            SessionExtensionsLocal.SetObject(httpContext.Session, "TZone", tz2);

            return SessionExtensionsLocal.GetObject<TZone>(httpContext.Session, "TZone");
        }


        public static IEnumerable<SelectListItem> GetFileNameSelectList(NotesDbContext db)
        {

            // Get a list of all files for dropdowns by name
            return db.NoteFile
                .OrderBy(c => c.NoteFileName)
                .Select(c => new SelectListItem
                {
                    Value = c.NoteFileName,
                    Text = c.NoteFileName
                });
        }

        public static IEnumerable<SelectListItem> GetFileTitleSelectList(NotesDbContext db)
        {

            // Get a list of all files for dropdowns by title
            return db.NoteFile
                .OrderBy(c => c.NoteFileTitle)
                .Select(c => new SelectListItem
                {
                    Value = c.NoteFileName,
                    Text = c.NoteFileTitle
                });
        }

        public static IEnumerable<SelectListItem> GetFileNameSelectListWithId(NotesDbContext db)
        {

            // Get a list of all files for dropdowns by name
            return db.NoteFile
                .OrderBy(c => c.NoteFileName)
                .Select(c => new SelectListItem
                {
                    Value = "" + $"{c.Id}",
                    Text = c.NoteFileName
                });
        }



    }

}
