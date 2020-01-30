using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Shared;

namespace Notes2021Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateNoteFileController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateNoteFileController(NotesDbContext db,
                UserManager<IdentityUser> userManager
                )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task Post(CreateFileModel crm)
        {
            IdentityUser me = await _userManager.FindByNameAsync(User.Identity.Name);

            await NoteDataManager.CreateNoteFile(_db, _userManager, me.Id, crm.NoteFileName, crm.NoteFileTitle);
        }



    }
}