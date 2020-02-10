/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: DataSpyController.cs
**
**  Description:
**      Data explorer Controller for Notes 2019
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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DataSpyController : Controller
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly UserManager<IdentityUser> _userManager;

        private readonly NotesDbContext _context;

        public DataSpyController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            NotesDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        // GET: DataSpy
        public async Task<IActionResult> Index()
        {
            await AccessManager.Audit(_context, "Index", User.Identity.Name,
                User.Identity.Name, "View list of NotesFiles");
            return View(await _context.NoteFile.ToListAsync());
        }

        // GET: DataSpy/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NoteFile noteFile = await _context.NoteFile.SingleAsync(m => m.Id == id);
            if (noteFile == null)
            {
                return NotFound();
            }

            await AccessManager.Audit(_context, "Details", User.Identity.Name,
                User.Identity.Name, "View details of NotesFile " + noteFile.NoteFileName);

            return View(noteFile);
        }


        public async Task<IActionResult> Headers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //List<NoteHeader> headers = await NoteDataManager.GetBaseNoteHeadersForFile(_context, (int)id);

            List<NoteHeader> headers = await _context.NoteHeader
                .Where(p => p.NoteFileId == id)
                .OrderBy(p => p.Id)
                .ToListAsync();

            await AccessManager.Audit(_context, "Headers", User.Identity.Name,
                User.Identity.Name, "View BaseNoteHeaders of NotesFileID " + id);

            return View(headers);
        }

        public async Task<IActionResult> Content(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            List<NoteHeader> headers = await _context.NoteHeader
                .Include("NoteContent")
                .Where(p => p.NoteFileId == id)
                .OrderBy(p => p.Id)
                .ToListAsync();

            List<NoteContent> content = new List<NoteContent>();
            foreach (var item in headers)
            {
                content.Add(item.NoteContent);
            }

            await AccessManager.Audit(_context, "Content", User.Identity.Name,
                User.Identity.Name, "View NoteContent of NotesFileID " + id);

            return View(content);
        }

        public async Task<IActionResult> EditContent(long id)
        {
            NoteContent model = await _context.NoteContent.SingleAsync(p => p.NoteHeaderId == id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContent(NoteContent nc)
        {
            NoteContent edited = await _context.NoteContent.SingleAsync(p => p.NoteHeaderId == nc.NoteHeaderId);

            edited.NoteBody = nc.NoteBody;
            edited.DirectorMessage = nc.DirectorMessage;

            await AccessManager.Audit(_context, "EditContent", User.Identity.Name,
                User.Identity.Name, "Edit Content NoteID " + edited.NoteHeaderId);

            _context.Entry(edited).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id = await NoteDataManager.GetNoteById(_context, nc.NoteHeaderId) });
        }

        public async Task<IActionResult> Access(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            List<NoteAccess> access = await _context.NoteAccess.Where(m => m.NoteFileId == id).ToListAsync();

            await AccessManager.Audit(_context, "Access", User.Identity.Name,
                User.Identity.Name, "View NoteAccess of NotesFileID " + id);

            return View(access);
        }

        public async Task<IActionResult> Marks()
        {
            List<Mark> mark = await _context.Mark.OrderBy(p => p.UserId).ToListAsync();

            await AccessManager.Audit(_context, "Marks", User.Identity.Name,
                User.Identity.Name, "View Marks");

            return View(mark);
        }

        public async Task<IActionResult> Search()
        {
            List<Search> s = await _context.Search.OrderBy(p => p.UserId).ToListAsync();

            await AccessManager.Audit(_context, "SearchView", User.Identity.Name,
                User.Identity.Name, "View SearchView");

            return View(s);
        }

        public async Task<IActionResult> Sequencer()
        {
            List<Sequencer> s = await _context.Sequencer.OrderBy(p => p.UserId).ToListAsync();

            await AccessManager.Audit(_context, "Sequencer", User.Identity.Name,
                User.Identity.Name, "View Sequencer");
            return View(s);
        }

        public async Task<IActionResult> Audit()
        {
            List<Audit> s = await _context.Audit.OrderByDescending(p => p.AuditID).ToListAsync();

            await AccessManager.Audit(_context, "Audit", User.Identity.Name,
                User.Identity.Name, "View Audit");

            return View(s);
        }

        public async Task<IActionResult> TimeZones()
        {
            List<TZone> s = await _context.TZone.OrderBy(p => p.OffsetHours).ThenBy(p => p.OffsetMinutes).ToListAsync();

            await AccessManager.Audit(_context, "TZone", User.Identity.Name,
                User.Identity.Name, "View TZone");

            return View(s);
        }

        public async Task<IActionResult> Users()
        {
            List<UserData> s = await _context.UserData.ToListAsync();    //_context.ApplicationUser.ToListAsync();

            await AccessManager.Audit(_context, "Users", User.Identity.Name,
                User.Identity.Name, "View Users");

            return View("Users2", s);
        }
    }
}
