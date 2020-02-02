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
    [Route("api/[controller]/{noteId}")]
    [ApiController]
    public class EnterAndDisplayController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public EnterAndDisplayController(NotesDbContext db,
                UserManager<IdentityUser> userManager
                )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<NoteHeader> Get(string noteId)
        {
            long Id = long.Parse(noteId);

            NoteHeader header = await _db.NoteHeader.Where(p => p.Id == Id).FirstOrDefaultAsync();

            return header;
        }

    }
}
