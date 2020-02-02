using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]/{fileId}")]
    [ApiController]
    public class MyAccessController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public MyAccessController(NotesDbContext db,
            UserManager<IdentityUser> userManager
            )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<NoteAccess> Get(string fileId)
        {
            int Id = int.Parse(fileId);

            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);

            NoteAccess mine = await _db.NoteAccess.Where(p => p.NoteFileId == Id && p.UserID == me.Id && p.ArchiveId == 0).OrderBy(p => p.ArchiveId).FirstOrDefaultAsync();

            if (mine == null)
            {
                mine = await _db.NoteAccess.Where(p => p.NoteFileId == Id && p.UserID == Globals.AccessOtherId() && p.ArchiveId == 0).OrderBy(p => p.ArchiveId).FirstOrDefaultAsync();
            }

            return mine;
        }

    }
}