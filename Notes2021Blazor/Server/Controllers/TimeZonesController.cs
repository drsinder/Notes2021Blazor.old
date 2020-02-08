using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeZonesController : ControllerBase
    {
        private readonly NotesDbContext _db;

        public TimeZonesController(NotesDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<List<TZone>> Get()
        {
            List<TZone> list = await _db.TZone.ToListAsync();

            return list;
        }

    }
}