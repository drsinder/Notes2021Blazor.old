using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly NotesDbContext _db;

        public UsersController(NotesDbContext db)
        {
            _db = db;

        }

        [HttpGet]
        public async Task<List<UserData>> Get()
        {
            List<UserData> list = await _db.UserData.ToListAsync();

            return list;
        }

    }
}