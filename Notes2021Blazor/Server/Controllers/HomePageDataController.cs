using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
            model.NoteFiles = _db.NoteFile
                .OrderBy(p => p.NoteFileName).ToList();

            List<UserData> udl = _db.UserData.ToList();

            foreach (NoteFile nf in model.NoteFiles)
            {
                UserData ud = udl.Find(p => p.UserId == nf.OwnerId);
                ud.MyGuid = "";
                ud.MyStyle = "";
                nf.Owner = ud;
            }

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