using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [Route("api/[controller]/{fileId}")]
    [ApiController]
    public class SequencerController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public SequencerController(NotesDbContext db,
            UserManager<IdentityUser> userManager
            )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<List<Sequencer>> Get()
        {
            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);

            List<Sequencer> mine = await _db.Sequencer.Where(p => p.UserId == me.Id).OrderBy(p => p.Ordinal).ThenBy(p => p.LastTime).ToListAsync();

            if (mine == null)
                mine = new List<Sequencer>();

            return mine;
        }

        [HttpPost]
        public async Task Post(SCheckModel model)
        {
            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);
            Sequencer tracker = new Sequencer
            {
                Active = true,
                NoteFileId = model.fileId,
                LastTime = DateTime.Now.ToUniversalTime(),
                UserId = me.Id,
                Ordinal = 0,
                StartTime = DateTime.Now.ToUniversalTime()
            };

            _db.Sequencer.Add(tracker);
            await _db.SaveChangesAsync();
        }

        [HttpDelete]
        public async Task Delete(int fileId)
        {
            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);
            Sequencer mine = await _db.Sequencer.SingleOrDefaultAsync(p => p.UserId == me.Id && p.NoteFileId == fileId);
            if (mine == null)
                return;

            _db.Sequencer.Remove(mine);
            await _db.SaveChangesAsync();
        }

        [HttpPut]
        public async Task Put(Sequencer seq)
        {
            Sequencer modified = await _db.Sequencer.SingleAsync(p => p.UserId == seq.UserId && p.NoteFileId == seq.NoteFileId);
            modified.LastTime = DateTime.Now.ToUniversalTime();
            _db.Entry(modified).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

    }
}
