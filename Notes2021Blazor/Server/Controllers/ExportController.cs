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
    [Route("api/[controller]/{modelstring}")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly NotesDbContext _db;

        public ExportController(NotesDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<List<NoteHeader>> Get(string modelstring)
        {
            int arcId;
            int fileId;
            int noteOrd;
            int respOrd;

            string[] parts = modelstring.Split(".");

            fileId = int.Parse(parts[0]);
            arcId = int.Parse(parts[1]);
            noteOrd = int.Parse(parts[2]);
            respOrd = int.Parse(parts[3]);

            List<NoteHeader> nhl;

            if (noteOrd == 0)
            {
                nhl = await _db.NoteHeader
                    .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.ResponseOrdinal == 0)
                    .OrderBy(p => p.NoteOrdinal)
                    .ToListAsync();
            }
            else
            {
                nhl = await _db.NoteHeader
                    .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal == respOrd)
                    .ToListAsync();
            }

            return nhl;
        }

    }
}