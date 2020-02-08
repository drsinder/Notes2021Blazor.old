using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(NotesDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<UserData> Get()
        {
            IdentityUser user = await _userManager.FindByNameAsync(User.Identity.Name);

            UserData me = await _db.UserData.SingleOrDefaultAsync(p => p.UserId == user.Id);

            return me;
        }

        [HttpPut]
        public async Task Put(UserData uData)
        {
            _db.Entry(uData).State = EntityState.Modified;
            int count = await _db.SaveChangesAsync();
        }


    }
}