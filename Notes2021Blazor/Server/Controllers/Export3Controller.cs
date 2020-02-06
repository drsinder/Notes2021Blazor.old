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
    public class Export3Controller : ControllerBase
    {
        private readonly NotesDbContext _db;

        public Export3Controller(NotesDbContext db )
        {
            _db = db;
        }

        [HttpGet]
        public async Task<List<NoteHeader>> Get(string modelstring)
        {
            int arcId;
            int fileId;
            int noteOrd;

            string[] parts = modelstring.Split(".");

            fileId = int.Parse(parts[0]);
            arcId = int.Parse(parts[1]);
            noteOrd = int.Parse(parts[2]);

            List<NoteHeader> nhl = await _db.NoteHeader
                .Include(m => m.NoteContent)
                .Include(m => m.Tags)
                .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal > 0)
                .OrderBy(p => p.ResponseOrdinal)
                .ToListAsync();

            return nhl;
        }

    }
}