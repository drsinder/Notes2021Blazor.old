using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Server.Services;
using Notes2021Blazor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [Route("api/[controller]/{modelstring}")]
    [ApiController]
    public class Export2Controller : ControllerBase
    {
        private readonly NotesDbContext _db;

        public Export2Controller(NotesDbContext db )
        {
            _db = db;
        }

        [HttpGet]
        public async Task<NoteContent> Get(string modelstring)
        {
            long noteid;

            noteid = long.Parse(modelstring);

            NoteContent nh = await _db.NoteContent
                //.Include("NoteContent")
                //.Include("Tags")
                .Where(p => p.NoteHeaderId == noteid)
                .FirstOrDefaultAsync();

            return nh;
        }

    }
}