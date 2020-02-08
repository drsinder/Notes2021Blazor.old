using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Server.Services;
using Notes2021Blazor.Shared;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class ForwardController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ForwardController(NotesDbContext db,
            UserManager<IdentityUser> userManager
          )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task Post(ForwardViewModel fv)
        {
            NoteHeader nh = await NoteDataManager.GetBaseNoteHeaderById(_db, fv.NoteID);

            IdentityUser usr = await _userManager.FindByNameAsync(User.Identity.Name);

            UserData ud = await _db.UserData.SingleOrDefaultAsync(p => p.UserId == usr.Id);

            string myEmail = await LocalService.MakeNoteForEmail(fv, _db, User.Identity.Name, ud.DisplayName);

            EmailSender emailSender = new EmailSender();

            await emailSender.SendEmailAsync(usr.UserName, fv.NoteSubject, myEmail);
        }

    }
}