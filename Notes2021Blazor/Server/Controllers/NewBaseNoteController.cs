using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Notes2021Blazor.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [Route("api/[controller]/{fileId}")]
    [ApiController]
    public class NewBaseNoteController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public NewBaseNoteController(NotesDbContext db,
            UserManager<IdentityUser> userManager
            )
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<NoteFile> Get(string fileId)
        {
            int Id = int.Parse(fileId);

            NoteFile nf  = _db.NoteFile.Single(p => p.Id == Id);

            return nf;
        }

        [HttpPost]
        public async Task<NoteHeader> Post(TextViewModel tvm)
        {
            if (tvm.MyNote == null)
                return null;

            UserData me = NoteDataManager.GetUserData(_userManager, User, _db);

            DateTime now = DateTime.Now.ToUniversalTime();
            NoteHeader nheader = new NoteHeader()
            {
                LastEdited = now,
                ThreadLastEdited = now,
                CreateDate = now,
                NoteFileId = tvm.NoteFileID,
                AuthorName = me.DisplayName,
                AuthorID = me.UserId,
                NoteSubject = tvm.MySubject,
                ResponseOrdinal = 0,
                ResponseCount = 0
            };

            NoteHeader created;

            if (tvm.BaseNoteHeaderID == 0)
            {
                created = await NoteDataManager.CreateNote(_db, _userManager, nheader, tvm.MyNote, tvm.TagLine, tvm.DirectorMessage, true, false);
            }
            else
            {
                nheader.BaseNoteId = tvm.BaseNoteHeaderID;
                created = await NoteDataManager.CreateResponse(_db, _userManager, nheader, tvm.MyNote, tvm.TagLine, tvm.DirectorMessage, true, false);
            }

            return created;
        }
    }
}