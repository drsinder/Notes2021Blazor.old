using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021.Api;
using Notes2021Blazor.Shared;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LinkedFilesController : NController
    {
        private readonly NotesDbContext _context;

        public LinkedFilesController(NotesDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager) : base(userManager, signInManager, context)
        {
            _context = context;
        }

        // GET: LinkedFiles
        public async Task<IActionResult> Index()
        {
            return View(await _context.LinkedFile.ToListAsync());
        }

        // GET: LinkedFiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linkedFile = await _context.LinkedFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (linkedFile == null)
            {
                return NotFound();
            }

            return View(linkedFile);
        }

        // GET: LinkedFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LinkedFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HomeFileId,HomeFileName,RemoteFileName,RemoteBaseUri,AcceptFrom,SendTo")] LinkedFile linkedFile)
        {
            if (ModelState.IsValid)
            {
                if (!linkedFile.RemoteBaseUri.EndsWith('/'))
                    linkedFile.RemoteBaseUri = linkedFile.RemoteBaseUri.TrimEnd(' ') + "/";

                LinkProcessor lp = new LinkProcessor(_context);

                if (await lp.Test(linkedFile.RemoteBaseUri))
                {
                    _context.Add(linkedFile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction(nameof(Error));
                }
            }
            return View(linkedFile);
        }

        // GET: LinkedFiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linkedFile = await _context.LinkedFile.FindAsync(id);
            if (linkedFile == null)
            {
                return NotFound();
            }
            return View(linkedFile);
        }

        // POST: LinkedFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HomeFileId,HomeFileName,RemoteFileName,RemoteBaseUri,AcceptFrom,SendTo")] LinkedFile linkedFile)
        {
            if (id != linkedFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!linkedFile.RemoteBaseUri.EndsWith('/'))
                        linkedFile.RemoteBaseUri = linkedFile.RemoteBaseUri.TrimEnd(' ') + "/";

                    LinkProcessor lp = new LinkProcessor(_context);

                    if (await lp.Test(linkedFile.RemoteBaseUri))
                    {
                        _context.Update(linkedFile);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return RedirectToAction(nameof(Error));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinkedFileExists(linkedFile.Id))
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
            return View(linkedFile);
        }

        // GET: LinkedFiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linkedFile = await _context.LinkedFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (linkedFile == null)
            {
                return NotFound();
            }

            return View(linkedFile);
        }

        // POST: LinkedFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var linkedFile = await _context.LinkedFile.FindAsync(id);
            _context.LinkedFile.Remove(linkedFile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LinkedFileExists(int id)
        {
            return _context.LinkedFile.Any(e => e.Id == id);
        }

        // GET:
        public IActionResult Error()
        {
            return View();
        }

    }
}
