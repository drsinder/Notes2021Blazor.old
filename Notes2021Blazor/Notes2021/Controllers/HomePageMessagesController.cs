/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: HomePageMessagesController.cs
**
**  Description:
**      HomePage Messages Controller for Notes 2019
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
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomePageMessagesController : NController
    {
        private readonly NotesDbContext _context;

        public HomePageMessagesController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            NotesDbContext context) : base(userManager, signInManager, context)

        {
            _context = context;
        }

        // GET: HomePageMessages
        public async Task<IActionResult> Index()
        {
            return View(await _context.HomePageMessage.OrderByDescending(p => p.Id).ToListAsync());
        }

        // GET: HomePageMessages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HomePageMessage homePageMessage = await _context.HomePageMessage.SingleAsync(m => m.Id == id);
            if (homePageMessage == null)
            {
                return NotFound();
            }

            return View(homePageMessage);
        }

        // GET: HomePageMessages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HomePageMessages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HomePageMessage homePageMessage)
        {
            if (ModelState.IsValid)
            {
                homePageMessage.Posted = DateTime.Now.ToUniversalTime();
                _context.HomePageMessage.Add(homePageMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(homePageMessage);
        }

        // GET: HomePageMessages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HomePageMessage homePageMessage = await _context.HomePageMessage.SingleAsync(m => m.Id == id);
            if (homePageMessage == null)
            {
                return NotFound();
            }
            return View(homePageMessage);
        }

        // POST: HomePageMessages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HomePageMessage homePageMessage)
        {
            if (ModelState.IsValid)
            {
                homePageMessage.Posted = DateTime.Now.ToUniversalTime();
                _context.Update(homePageMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(homePageMessage);
        }

        // GET: HomePageMessages/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HomePageMessage homePageMessage = await _context.HomePageMessage.SingleAsync(m => m.Id == id);
            if (homePageMessage == null)
            {
                return NotFound();
            }

            return View(homePageMessage);
        }

        // POST: HomePageMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            HomePageMessage homePageMessage = await _context.HomePageMessage.SingleAsync(m => m.Id == id);
            _context.HomePageMessage.Remove(homePageMessage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
