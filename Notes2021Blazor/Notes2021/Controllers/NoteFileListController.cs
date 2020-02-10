/*--------------------------------------------------------------------------
    **
    **  Copyright © 2019, Dale Sinder
    **
    **  Name: NoteFileListController.cs
    **
    **  Description:
    **      NoteFile List Controller
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2021.Manager;
using Notes2021Blazor.Shared;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "User")]
    public class NoteFileListController : NController
    {

        public NoteFileListController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            NotesDbContext NotesDbContext) : base(userManager, signInManager, NotesDbContext)
        {
        }

        // GET: NoteFileList
        /// <summary>
        /// Display list of NoteFiles
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetInt32("IsSearch", 0);

            TZone tz = await LocalManager.GetUserTimeZone(Request.HttpContext, User, _userManager, _signInManager, _db);
            ViewBag.TZ = tz;

            List<NoteFile> nf = await NoteDataManager.GetNoteFilesOrderedByName(_db);
            return View(nf);
        }

        // GET: NoteFileList/Details/5
        /// <summary>
        /// Show some info about the NoteFile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            NoteFile noteFile = await NoteDataManager.GetFileByIdWithOwner(_db, (int)id);
            if (noteFile == null)
            {
                return NotFound();
            }

            TZone tz = await LocalManager.GetUserTimeZone(Request.HttpContext, User, _userManager, _signInManager, _db);
            ViewBag.TZ = tz;

            ViewBag.BaseNotes = await NoteDataManager.NextBaseNoteOrdinal(_db, (int)id, 0) - 1;
            ViewBag.TotalNotes = await NoteDataManager.GetNumberOfNotes(_db, (int)id, 0);
            return View(noteFile);
        }

        /// <summary>
        /// Shim to Enter a NoteFile from the list of NoteFiles
        /// </summary>
        /// <param name="id">NoteFile</param>
        /// <returns></returns>
        public async Task<IActionResult> Viewit(int? id)
        {
            HttpContext.Session.SetInt32("IsSearch", 0);

            if (id == null)
            {
                return NotFound();
            }

            NoteFile noteFile = await NoteDataManager.GetFileById(_db, (int)id);

            if (noteFile == null)
            {
                return NotFound();
            }

            // Check access
            NoteAccess nacc = await GetMyAccess((int)id);
            if (!nacc.Write && !nacc.ReadAccess) // can not read or write = no access
                return RedirectToAction("Index");

            HttpContext.Session.SetInt32("ArchiveID", Convert.ToInt32(0));

            return RedirectToAction("Listing", "NoteDisplay", new { id = noteFile.Id });
        }

        /// <summary>
        /// Get Access for User in a file
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        private async Task<NoteAccess> GetMyAccess(int fileid)
        {
            NoteAccess noteAccess = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), fileid, 0);
            ViewData["MyAccess"] = noteAccess;
            return noteAccess;
        }


    }
}