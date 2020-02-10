/*--------------------------------------------------------------------------
    **
    **  Copyright © 2019, Dale Sinder
    **
    **  Name: NoteFilesController.cs
    **
    **  Description:
    **      Note Files Controller
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NoteFilesController : NController
    {

        public NoteFilesController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, NotesDbContext context) : base(userManager, signInManager, context)
        {
        }

        // GET: NoteFiles
        /// <summary>
        /// Display list of NoteFiles
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            NoteFile nf1 = await NoteDataManager.GetFileByName(_db, "announce");
            NoteFile nf2 = await NoteDataManager.GetFileByName(_db, "pbnotes");
            NoteFile nf3 = await NoteDataManager.GetFileByName(_db, "noteshelp");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "pad");

            ViewBag.announce = nf1 == null;
            ViewBag.pbnotes = nf2 == null;
            ViewBag.noteshelp = nf3 == null;
            ViewBag.pad = nf4 == null;

            return View(await NoteDataManager.GetNoteFilesOrderedByNameWithOwner(_db));
        }

        /// <summary>
        /// Create a New NoteFile with default Access Controls.
        /// "Other" has no access.  Creator has full access.
        /// </summary>
        /// <param name="name">Name for the file</param>
        /// <param name="title">Title of the file</param>
        /// <returns></returns>
        public async Task<bool> CreateNoteFile(string name, string title)
        {
            await AccessManager.Audit(_db, "Create", User.Identity.Name, _userManager.GetUserId(User), "Create NotesFile " + name);

            return await NoteDataManager.CreateNoteFile(_db, _userManager, _userManager.GetUserId(User), name, title);
        }

        public async Task<IActionResult> CreateAnnounce()
        {
            await CreateNoteFile("announce", "Notes 2021 Announcements");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "announce");
            int padid = nf4.Id;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Globals.AccessOtherId(), padid, 0);
            access.ReadAccess = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreatePbnotes()
        {
            await CreateNoteFile("pbnotes", "Public Notes");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "pbnotes");
            int padid = nf4.Id;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Globals.AccessOtherId(), padid, 0);
            access.ReadAccess = true;
            access.Respond = true;
            access.Write = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreateNoteshelp()
        {
            await CreateNoteFile("noteshelp", "Help with Notes 2021");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "noteshelp");
            int padid = nf4.Id;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Globals.AccessOtherId(), padid, 0);
            access.ReadAccess = true;
            access.Respond = true;
            access.Write = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreatePad()
        {
            await CreateNoteFile("pad", "Traditional Pad");
            NoteFile nf4 = await NoteDataManager.GetFileByName(_db, "pad");
            int padid = nf4.Id;
            NoteAccess access = await AccessManager.GetOneAccess(_db, Globals.AccessOtherId(), padid, 0);
            access.ReadAccess = true;
            access.Respond = true;
            access.Write = true;

            _db.Entry(access).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // GET: NoteFiles/Details/5
        /// <summary>
        /// Dislplay details about a file
        /// </summary>
        /// <param name="id"></param>
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
            ViewBag.BaseNotes = await NoteDataManager.NextBaseNoteOrdinal(_db, (int)id, 0) - 1;
            ViewBag.TotalNotes = await NoteDataManager.GetNumberOfNotes(_db, (int)id, 0);
            //ViewBag.TotalLength = await NoteDataManager.GetFileLength(_db, (int)id);

            return View(noteFile);
        }



        // GET: NoteFiles/Create
        public IActionResult Create()
        {
            ViewData["OwnerId"] = new SelectList(_db.UserData, "UserId", "DisplayName");
            return View();
        }

        // POST: NoteFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Process creation of a new NoteFile
        /// </summary>
        /// <param name="noteFile">NoteFile data from View</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(/*[Bind(Include = "NoteFileID,NoteFileName,NoteFileTitle,LastEdited")]*/ NoteFile noteFile)
        {
            if (ModelState.IsValid)
            {
                await AccessManager.Audit(_db, "Create", User.Identity.Name, _userManager.GetUserId(User), "Create NotesFile " + noteFile.NoteFileName);

                if (!await NoteDataManager.CreateNoteFile(_db, _userManager, _userManager.GetUserId(User), noteFile.NoteFileName, noteFile.NoteFileTitle))
                    return View(noteFile);

                await AccessManager.Audit(_db, "Failed", User.Identity.Name, _userManager.GetUserId(User), "Create NotesFile " + noteFile.NoteFileName);

                return RedirectToAction("Index");
            }

            return View(noteFile);
        }


        // GET: NoteFiles/Edit/5
        /// <summary>
        /// Set up to edit a NoteFile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
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
            ViewData["OwnerIds"] = new SelectList(_db.UserData, "UserId", "DisplayName", noteFile.OwnerId);

            return View(noteFile);
        }


        // POST: NoteFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NumberArchives,OwnerId,NoteFileName,NoteFileTitle,LastEdited")] NoteFile noteFile)
        {
            if (id != noteFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await AccessManager.Audit(_db, "Edit", User.Identity.Name, _userManager.GetUserId(User), "Edit NotesFile " + noteFile.NoteFileName);
                    noteFile.LastEdited = DateTime.Now.ToUniversalTime();
                    _db.Update(noteFile);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteFileExists(noteFile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OwnerId = new SelectList(_db.UserData, "UserId", "DisplayName", noteFile.OwnerId);
            return View(noteFile);
        }

        // GET: NoteFiles/Delete/5
        /// <summary>
        /// Setup to  delete a NoteFile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
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
            return View(noteFile);
        }

        // POST: NoteFiles/Delete/5
        /// <summary>
        /// Process deletion of notefile
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            NoteFile noteFile = await NoteDataManager.GetFileById(_db, id);
            await AccessManager.Audit(_db, "Delete", User.Identity.Name, _userManager.GetUserId(User), "Delete NotesFile " + noteFile.NoteFileName);

            await NoteDataManager.DeleteNoteFile(_db, id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Archive(int? id)
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
            ViewData["OwnerId"] = new SelectList(_db.Users, "Id", "DisplayName", noteFile.OwnerId);

            return View(noteFile);
        }


        public async Task<IActionResult> DoArchive(int? id)
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

            NoteDataManager.ArchiveNoteFile(_db, noteFile);

            //noteFile.NumberArchives++;
            //_db.Update(noteFile);

            //List<NoteHeader> nhl = await _db.NoteHeader.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToListAsync();

            //foreach (NoteHeader nh in nhl)
            //{
            //    nh.ArchiveId = noteFile.NumberArchives;
            //    _db.Update(nh);
            //}

            //List<NoteAccess> nal = await _db.NoteAccess.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToListAsync();
            //foreach (NoteAccess na in nal)
            //{
            //    na.ArchiveId = noteFile.NumberArchives;
            //}
            //_db.NoteAccess.AddRange(nal);

            //List<Tags> ntl = await _db.Tags.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToListAsync();
            //foreach (Tags nt in ntl)
            //{
            //    nt.ArchiveId = noteFile.NumberArchives;
            //    _db.Update(nt);
            //}


            //await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        private bool NoteFileExists(int id)
        {
            return _db.NoteFile.Any(e => e.Id == id);
        }
    }

    //public class FileViewModel
    //{
    //    NoteFile nf;
    //    SelectList users;
    //}
}
