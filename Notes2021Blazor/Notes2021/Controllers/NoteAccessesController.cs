/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: NoteAccessesController.cs
**
**  Description:
**      Note Accesses Controller for Notes 2019
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "User")]
    public class NoteAccessesController : NController
    {
        public NoteAccessesController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            NotesDbContext NotesDbContext) : base(userManager, signInManager, NotesDbContext)
        {
        }

        // Get Access Control Object for User to do permission checks
        public async Task<NoteAccess> GetMyAccess(int fileid, int arcId)
        {
            NoteAccess noteAccess = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), fileid, arcId);
            ViewData["MyAccess"] = noteAccess;
            return noteAccess;
        }

        /// <summary>
        /// Get the user name given the userid
        /// </summary>
        /// <param name="id">userid</param>
        /// <returns>username</returns>
        public string GetUserNameFromID(string id)
        {
            if (Globals.AccessOtherId() == id)
                return Globals.AccessOther();

            string myname = _db.UserData
                .FirstOrDefault(p => p.UserId == id)
                ?.DisplayName;

            if (string.IsNullOrEmpty(myname))
                return "??";
            return myname;
        }


        /// <summary>
        /// Get the UserID given the user name
        /// </summary>
        /// <param name="name">username</param>
        /// <returns>UserID</returns>
        // ReSharper disable once UnusedMember.Local
        private async Task<string> GetUserIDFromName(string name)
        {
            var user = await _db.UserData
                .Where(p => p.DisplayName == name)
                .FirstOrDefaultAsync();

            if (user == null)
                return "";

            return user.UserId;
        }

        // GET: NoteAccesses
        /// <summary>
        /// Index of Access Controls for a NoteFile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int id)
        {

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            //Check if user has right to Edit or View the List
            NoteAccess myaccess = await GetMyAccess(id, arcId);
            if (!myaccess.EditAccess && !myaccess.ViewAccess)
                return RedirectToAction("Index", "Home");

            // Get the Object for the NoteFile
            ViewBag.NoteFileID = id;
            NoteFile nf = await NoteDataManager.GetFileById(_db, id);

            ViewBag.NoteFileName = nf.NoteFileName;

            // Get the list of entries
            List<NoteAccess> thelist = await AccessManager.GetAccessListForFile(_db, nf.Id, arcId);

            List<string> names = new List<string>();
            List<string> ds = new List<string>();
            foreach (NoteAccess item in thelist)
            {
                names.Add(GetUserNameFromID(item.UserID));
            }

            ds.Add(_userManager.GetUserId(User));
            ds.Add(Globals.AccessOtherId());

            ViewBag.names = names;
            ViewBag.IDs = ds;

            return View(thelist);
        }



        // GET: NoteAccesses/Create
        /// <summary>
        /// Create a new access entry for a user - Set up
        /// </summary>
        /// <param name="id">UserID</param>
        /// <returns></returns>
        public async Task<IActionResult> Create(int id)
        {
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteAccess myaccess = await GetMyAccess(id, arcId);
            if (!myaccess.EditAccess)
                return RedirectToAction("Index");
            ViewBag.MyAccess = myaccess;

            NoteFile nf = await NoteDataManager.GetFileById(_db, id);

            ViewBag.NoteFileName = nf.NoteFileName;

            // Get list of usernames for dropdown
            IEnumerable<SelectListItem> items = _db.UserData.OrderBy(c => c.UserId).Select(c => new SelectListItem
            {
                Value = c.UserId.ToString(),
                Text = c.DisplayName
            });
            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem {Value = "", Text = "-- Select a User --"}
            };
            list2.AddRange(items);
            ViewBag.UserList = list2;

            // setup a new entry pass to view
            NoteAccess na = new NoteAccess { NoteFileId = id };
            // set the NoteFileID
            return View(na);
        }

        // POST: NoteAccesses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Handle creation of new access control entry
        /// </summary>
        /// <param name="noteAccess"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NoteAccess noteAccess)
        {
            if (ModelState.IsValid)
            {
                if (noteAccess.UserID == "")
                {
                    ViewBag.ErrorMessage = "Not a valid user name!";
                    return View(noteAccess);
                }

                string myid = noteAccess.UserID;  // await GetUserIDFromName(noteAccess.UserID);
                if (myid == "")
                {
                    ViewBag.ErrorMessage = "Not a valid user id!";
                    return View(noteAccess);
                }

                noteAccess.UserID = myid;  // set the userid

                try  // add new entry to DB
                {
                    _db.NoteAccess.Add(noteAccess);
                    await _db.SaveChangesAsync();
                }
                catch
                {
                    ViewBag.ErrorMessage = "Is that user already on the list?";
                }

                return RedirectToAction("Index", new { id = noteAccess.NoteFileId });
            }

            return View(noteAccess);
        }


        // GET: NoteAccesses/Edit/5
        /// <summary>
        /// Edit an access control for a user and file
        /// </summary>
        /// <param name="id">UserID</param>
        /// <param name="id2">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(string id, int id2)
        {
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteAccess myaccess = await GetMyAccess(id2, arcId);
            if (!myaccess.EditAccess)
                return RedirectToAction("Index");
            ViewBag.MyAccess = myaccess;

            // get the Object for the User/NoteFile
            NoteAccess noteAccess = await AccessManager.GetOneAccess(_db, id, id2, arcId);

            if (noteAccess == null)
            {
                return NotFound();
            }
            // Get the NoteFile Name
            NoteFile nf = await NoteDataManager.GetFileById(_db, id2);

            ViewBag.NoteFileName = nf.NoteFileName;
            ViewBag.NoteUserName = GetUserNameFromID(id);

            // Pass Access Control Object to View
            return View(noteAccess);
        }

        // POST: NoteAccesses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Update the Access Control Object from view data
        /// </summary>
        /// <param name="noteAccess">Access Control Object from view data</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NoteAccess noteAccess)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(noteAccess).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = noteAccess.NoteFileId });
            }
            return View(noteAccess);
        }


        // GET: NoteAccesses/Delete/5
        /// <summary>
        /// Delete an Accese Control Object
        /// </summary>
        /// <param name="id">UserID</param>
        /// <param name="id2">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(string id, int id2)
        {
            if (id == "")
            {
                return NotFound();
            }
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteAccess myaccess = await GetMyAccess(id2, arcId);
            if (!myaccess.EditAccess)
                return RedirectToAction("Index", "Home");
            ViewBag.MyAccess = myaccess;
            // Get Access Control Object
            NoteAccess noteAccess = await AccessManager.GetOneAccess(_db, id, id2, arcId);

            if (noteAccess == null)
            {
                return NotFound();
            }
            // Get NoteFile Object
            ViewBag.NoteFileName = _db.NoteFile
                .FirstOrDefault(p => p.Id == id2)
                ?.NoteFileName;

            ViewBag.NoteUserName = GetUserNameFromID(id);
            // Send Access Control Object to View
            return View(noteAccess);
        }

        // POST: NoteAccesses/Delete/5
        /// <summary>
        /// Performs delete of Access Control Object
        /// </summary>
        /// <param name="id">UserID</param>
        /// <param name="id2">NoteFileID</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, int id2)
        {
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteAccess noteAccess = await AccessManager.GetOneAccess(_db, id, id2, arcId);

            _db.NoteAccess.Remove(noteAccess);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = noteAccess.NoteFileId });
        }

        // ReSharper disable once UnusedMember.Local
        private bool NoteAccessExists(string id)
        {
            return _db.NoteAccess.Any(e => e.UserID == id);
        }
    }
}
