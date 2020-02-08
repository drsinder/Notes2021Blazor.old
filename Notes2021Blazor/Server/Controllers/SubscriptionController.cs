using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [Route("api/[controller]/{fileId}")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public SubscriptionController(NotesDbContext db,
            UserManager<IdentityUser> userManager
            )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<List<Subscription>> Get()
        {
            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);

            List<Subscription> mine = await _db.Subscription.Where(p => p.SubscriberId == me.Id).ToListAsync();

            if (mine == null)
                mine = new List<Subscription>();

            return mine;
        }

        [HttpPost]
        public async Task Post(SCheckModel model)
        {
            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);
            Subscription sub = new Subscription
            {
                NoteFileId = model.fileId,
                SubscriberId = me.Id,
            };

            _db.Subscription.Add(sub);
            await _db.SaveChangesAsync();
        }

        [HttpDelete]
        public async Task Delete(int fileId)
        {
            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);
            Subscription mine = await _db.Subscription.SingleOrDefaultAsync(p => p.SubscriberId == me.Id && p.NoteFileId == fileId);
            if (mine == null)
                return;

            _db.Subscription.Remove(mine);
            await _db.SaveChangesAsync();
        }

    }
}
