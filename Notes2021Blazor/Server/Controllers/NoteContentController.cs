using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Shared;

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
        public async Task<NoteContent> Get(string sid)
        {
            long id = long.Parse(sid);

            NoteContent c = _db.NoteContent.Single(p => p.NoteHeaderId == id);
            return c;
        }

    }
}