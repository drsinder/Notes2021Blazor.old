﻿/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: SequencersController.cs
**
**  Description:
**      Sequencers Controller for Notes 2020
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
using Notes2021.Manager;

using Notes2021Blazor.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "User")]
    public class SequencersController : NController
    {
        public SequencersController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            NotesDbContext NotesDbContext) : base(userManager, signInManager, NotesDbContext)
        {
        }

        public async Task<int> GetIdFromFileName(string filename)
        {
            NoteFile noteFile = await NoteDataManager.GetFileByName(_db, filename);
            return noteFile.Id;
        }

        public async Task<string> GetFileNameFromId(int id)
        {
            NoteFile noteFile = await NoteDataManager.GetFileById(_db, id);

            return noteFile.NoteFileName;
        }

        public async Task<int> GetNextOrdinal(string myid)
        {
            List<Sequencer> myseq = await _db.Sequencer
                .Where(p => p.UserId == myid)
                .OrderByDescending(p => p.Ordinal)
                .ToListAsync();

            if (myseq == null || !myseq.Any())
                return 1;

            return myseq.First().Ordinal + 1;
        }

        // GET: Sequencers
        public async Task<IActionResult> Index()
        {
            string userid = _userManager.GetUserId(User);

            ViewBag.MaxOrdinal = await GetNextOrdinal(userid) - 1;

            List<Sequencer> it = await _db.Sequencer
                .Where(p => p.UserId == userid)
                .OrderBy(p => p.Ordinal)
                .ToListAsync();

            List<string> names = new List<string>();
            foreach (Sequencer item in it)
            {
                names.Add(await GetFileNameFromId(item.NoteFileId));
            }

            ViewData["Names"] = names;

            return View(it);
        }

        // GET: Sequencers/Create
        public IActionResult Create()
        {
            // Get a list of all file names for dropdown
            IEnumerable<SelectListItem> items = LocalManager.GetFileNameSelectList(_db);
            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a File --"
                }
            };

            list2.AddRange(items);

            Notes2021.Models.ExportViewModel it = new Notes2021.Models.ExportViewModel { AFiles = list2 };

            // Get a list of all file titles for dropdown
            items = LocalManager.GetFileTitleSelectList(_db);
            list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "-- Select a Title --"
                }
            };
            list2.AddRange(items);
            it.ATitles = list2;

            return View(it);
        }

        // POST: Sequencers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notes2021.Models.ExportViewModel fn)
        {
            int fileid = await GetIdFromFileName(fn.FileName);
            if (fileid == 0)
                return RedirectToAction("Index");

            string myid = _userManager.GetUserId(User);

            Sequencer sequencer = new Sequencer
            {
                Active = false,
                LastTime = DateTime.Now.ToUniversalTime(),
                NoteFileId = fileid,
                Ordinal = await GetNextOrdinal(myid),
                StartTime = DateTime.Now.ToUniversalTime(),
                UserId = myid
            };

            try
            {
                _db.Sequencer.Add(sequencer);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // ignored
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MoveUp(string id, int? id2)
        {
            Sequencer sequencer = await _db.Sequencer
                .Where(p => p.UserId == id && p.NoteFileId == id2)
                .FirstOrDefaultAsync();

            if (sequencer.Ordinal == 1)
                return RedirectToAction("Index");

            List<Sequencer> myseq = await NoteDataManager.GetSeqListForUser(_db, id);

            foreach (Sequencer s in myseq)
            {
                if (s.Ordinal == sequencer.Ordinal - 1)
                {
                    s.Ordinal++;
                    _db.Entry(s).State = EntityState.Modified;
                }
                else if (s.Ordinal == sequencer.Ordinal)
                {
                    s.Ordinal--;
                    _db.Entry(s).State = EntityState.Modified;
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MoveDown(string id, int? id2)
        {
            Sequencer sequencer = await _db.Sequencer
                .Where(p => p.UserId == id && p.NoteFileId == id2)
                .FirstOrDefaultAsync();

            if (sequencer.Ordinal == (await GetNextOrdinal(id) - 1))
                return RedirectToAction("Index");

            List<Sequencer> myseq = await NoteDataManager.GetSeqListForUser(_db, id);

            bool changedone = false;

            foreach (Sequencer s in myseq)
            {
                if (s.Ordinal == sequencer.Ordinal && !changedone)
                {
                    s.Ordinal++;
                    _db.Entry(s).State = EntityState.Modified;
                    changedone = true;
                }
                else if (s.Ordinal == sequencer.Ordinal && changedone)
                {
                    s.Ordinal--;
                    _db.Entry(s).State = EntityState.Modified;
                    break;
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Sequencers/Delete/5
        public async Task<IActionResult> Delete(string id, int? id2)
        {
            if (id2 == null)
            {
                return NotFound();
            }
            Sequencer sequencer = await _db.Sequencer
                .Where(p => p.UserId == id && p.NoteFileId == id2)
                .FirstOrDefaultAsync();

            if (sequencer == null)
            {
                return NotFound();
            }

            ViewData["Name"] = GetFileNameFromId(sequencer.NoteFileId);

            return View(sequencer);
        }

        // POST: Sequencers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, int id2)
        {
            Sequencer sequencer = await _db.Sequencer
                .Where(p => p.UserId == id && p.NoteFileId == id2)
                .FirstOrDefaultAsync();

            _db.Sequencer.Remove(sequencer);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Sequencers/Edit/5
        public async Task<IActionResult> Edit(string id, int? id2)
        {
            if (id2 == null)
            {
                return NotFound();
            }

            int nf = (int)id2;

            Sequencer sequencer = await _db.Sequencer
                .Where(p => p.UserId == id && p.NoteFileId == id2)
                .FirstOrDefaultAsync();

            if (sequencer == null)
            {
                return NotFound();
            }

            NoteFile notefile = await _db.NoteFile
                .Where(p => p.Id == nf)
                .FirstOrDefaultAsync();

            ViewData["NoteFileName"] = notefile.NoteFileName;

            return View(sequencer);
        }

        // POST: Sequencers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Sequencer sequencer)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(sequencer).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sequencer);
        }
    }
}
