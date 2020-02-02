using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [Route("api/[controller]/{sid}")]
    [ApiController]
    public class NoteContentController : ControllerBase
    {
        private readonly NotesDbContext _db;

        public NoteContentController(NotesDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<DisplayModel> Get(string sid)
        {
            long id = long.Parse(sid);

            NoteContent c = _db.NoteContent.Single(p => p.NoteHeaderId == id);
            List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == id).ToListAsync();

            return new DisplayModel { content = c, tags = tags };
        }

    }
}