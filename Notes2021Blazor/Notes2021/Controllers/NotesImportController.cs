/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: NotesImportController.cs
**
**  Description:
**      Notes Import Controller for Notes 2020
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Notes2021Blazor.Shared;

using System.Threading.Tasks;
using PusherServer;

namespace Notes2021.Controllers
{
    public partial class Importer : Import.Importer
    {
        public HttpContext context;
        public override void Output(string message)
        {
            var options = new PusherOptions
            {
                Encrypted = true,
                Cluster = Globals.PusherCluster

            };
            var pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);

            string newmessage = context.Session.GetString("ImportStatus") + "   " + message;
            context.Session.SetString("ImportStatus", newmessage);

            pusher.TriggerAsync("notes-channel", "import_status_message_event", new { newmessage });
        }
    }

    [Authorize(Roles = "Admin")]
    public class NotesImportController : NController
    {
        private readonly IWebHostEnvironment _appEnv;

        public NotesImportController(
            IWebHostEnvironment appEnv,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            NotesDbContext NotesDbContext) : base(userManager, signInManager, NotesDbContext)
        {
            _appEnv = appEnv;
        }

        // GET: NotesImport
        /// <summary>
        /// Display list of NoteFiles user can import to
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return View(await NoteDataManager.GetNoteFilesOrderedByName(_db));
        }

        public async Task<IActionResult> StartImport(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            NoteFile noteFile = await NoteDataManager.GetFileById(_db, (int)id);

            if (noteFile == null)
            {
                return NotFound();
            }

            return View(noteFile);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(NoteFile nf)
        {
            if (nf == null)
            {
                return NotFound();
            }
            NoteFile noteFile = await NoteDataManager.GetFileById(_db, nf.Id);

            if (noteFile == null)
            {
                return NotFound();
            }

            string fileName = _appEnv.ContentRootPath + "\\wwwroot\\ImportFiles\\" + noteFile.NoteFileName + ".txt";
            int id = noteFile.Id;

            Importer imp = new Importer();

            imp.context = HttpContext;
            HttpContext.Session.SetString("ImportStatus", string.Empty);

            await imp.Import(_db, fileName, noteFile.NoteFileName);

            return RedirectToAction("Listing", "NoteDisplay", new { id });
        }



    }
}
