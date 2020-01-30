using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notes2021Blazor.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Blazored.LocalStorage;

namespace Notes2021Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePageDataController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public HomePageDataController(NotesDbContext db, 
            UserManager<IdentityUser> userManager
            )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<HomePageModel> Get()
        {
            HomePageModel model = new HomePageModel();

            model.TimeZone = _db.TZone.Single(p => p.Id == Globals.TimeZoneDefaultID);
            model.Message = _db.HomePageMessage.FirstOrDefault();
            model.NoteFiles = _db.NoteFile.OrderBy(p => p.NoteFileName).ToList();

            try
            {
                string userName = this.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                if (!string.IsNullOrEmpty(userName))
                {
                    IdentityUser user = await _userManager.FindByNameAsync(userName);
                    model.UserData = _db.UserData.Single(p => p.UserId == user.Id);
                }
                else
                {
                    model.UserData = new UserData { TimeZoneID = Globals.TimeZoneDefaultID };
                }
            }
            catch
            {
                model.UserData = new UserData { TimeZoneID = Globals.TimeZoneDefaultID };
            }

            return model;
        }
    }
}