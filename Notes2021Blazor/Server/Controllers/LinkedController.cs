using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Server.Services;
using Notes2021Blazor.Shared;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [Route("api/[controller]/{Id}")]
    [ApiController]
    public class LinkedController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public LinkedController(NotesDbContext db,
            UserManager<IdentityUser> userManager
          )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<List<LinkedFile>> Get()
        {
            return await _db.LinkedFile.ToListAsync();
        }

        [HttpPost] 
        public async Task Post(LinkedFile linkedFile)
        {
            _db.LinkedFile.Add(linkedFile);
            await _db.SaveChangesAsync();
        }

        [HttpPut] 
        public async Task Put(LinkedFile linkedFile)
        {
            _db.Entry(linkedFile).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        [HttpDelete] 
        public async Task Delete(string Id)
        {
            int myId = int.Parse(Id);
            LinkedFile myFile = await _db.LinkedFile.SingleOrDefaultAsync(p => p.Id == myId);
            _db.LinkedFile.Remove(myFile);
            await _db.SaveChangesAsync();
        }

    }
}