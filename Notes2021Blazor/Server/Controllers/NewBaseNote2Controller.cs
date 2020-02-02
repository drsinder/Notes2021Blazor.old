using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Shared;

namespace Notes2021Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewBaseNote2Controller : ControllerBase
    {
        private readonly NotesDbContext _db;

        public NewBaseNote2Controller(NotesDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<NoteHeader> Get()
        {
            NoteHeader nh = _db.NoteHeader.OrderByDescending(p => p.Id).FirstOrDefault();
            return nh;
        }
    }
}
