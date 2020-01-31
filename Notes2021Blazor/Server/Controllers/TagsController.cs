using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

namespace Notes2021Blazor.Server.Controllers
{
    [Route("api/[controller]/{sid}")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly NotesDbContext _db;

        public TagsController(NotesDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<List<Tags>> Get(string sid)
        {
            long Id = long.Parse(sid);

            return await _db.Tags.Where(p => p.NoteHeaderId == Id).ToListAsync();
        }
    }
}