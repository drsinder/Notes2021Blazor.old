/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: RoleController.cs
**
**  Description:
**      Role Controller for Notes 2020
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : NController
    {

        public RoleController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            //IEmailSender emailSender,
            //ISmsSender smsSender,
            NotesDbContext NotesDbContext) : base(userManager, signInManager, NotesDbContext)
        {
        }

        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            List<IdentityRole> roles = await _db.Roles.OrderBy(p => p.Name).ToListAsync();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            role.NormalizedName = role.Name.ToUpper();
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await _db.Roles.Where(p => p.Id == id).FirstOrDefaultAsync();
            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            IdentityRole role = await _db.Roles.Where(p => p.Id == id).FirstOrDefaultAsync();
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
