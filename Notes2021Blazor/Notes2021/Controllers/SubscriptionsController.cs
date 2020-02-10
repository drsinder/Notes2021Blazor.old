/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: SubscriptionsController.cs
**
**  Description:
**      Subscriptions Controller for Notes 2020
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "User")]
    public class SubscriptionsController : NController
    {
        private readonly NotesDbContext _context;

        public SubscriptionsController(NotesDbContext context, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager) : base(userManager, signInManager, context)
        {
            _context = context;
        }

        // GET: Subscriptions
        public async Task<IActionResult> Index()
        {
            ViewBag.Db = _context;
            ViewBag.UserID = _userManager.GetUserId(User);
            ViewBag.UserSafeName = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db);
            return View(await _context.Subscription
                .Include("NoteFile")
                .Where(p => p.SubscriberId == _userManager.GetUserId(User))
                .ToListAsync());
        }

        //// GET: Subscriptions/Details/5
        //public async Task<IActionResult> Details(long? id)
        //{
        //    if (id == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    Subscription subscription = await _context.Subscription.SingleAsync(m => m.Id == id);
        //    if (subscription == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(subscription);
        //}

        // GET: Subscriptions/Create
        public IActionResult Create()
        {
            ViewBag.Db = _context;
            ViewBag.UserID = _userManager.GetUserId(User);

            IEnumerable<SelectListItem> files = LocalManager.GetFileNameSelectListWithId(_context);
            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem {Value = "", Text = "-- Select a File --"}
            };

            list2.AddRange(files);

            ViewBag.FilesList = list2;

            return View();
        }

        // POST: Subscriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subscription subscription)
        {
            if (ModelState.IsValid)
            {
                _context.Subscription.Add(subscription);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Db = _context;
            ViewBag.UserID = _userManager.GetUserId(User);

            IEnumerable<SelectListItem> files = LocalManager.GetFileNameSelectListWithId(_context);
            ViewBag.FilesList = files;

            return View(subscription);
        }

        //// GET: Subscriptions/Edit/5
        //public async Task<IActionResult> Edit(long? id)
        //{
        //    if (id == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.Db = _context;
        //    ViewBag.UserID = User.GetUserId();

        //    Subscription subscription = await _context.Subscription.SingleAsync(m => m.Id == id);
        //    if (subscription == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(subscription);
        //}

        //// POST: Subscriptions/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Subscription subscription)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Update(subscription);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    return View(subscription);
        //}

        // GET: Subscriptions/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Subscription subscription = await _context.Subscription
                .Include("NoteFile")
                .SingleAsync(m => m.Id == id);
            if (subscription == null)
            {
                return NotFound();
            }
            ViewBag.Db = _context;

            return View(subscription);
        }

        // POST: Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Subscription subscription = await _context.Subscription.SingleAsync(m => m.Id == id);
            _context.Subscription.Remove(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
