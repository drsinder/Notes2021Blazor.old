﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Shared;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [Route("api/[controller]/{id}")]
    [ApiController]
    public class NoteFileAdminController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public NoteFileAdminController(NotesDbContext db,
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

        [HttpDelete]
        public async Task Delete(string id)
        {
            int intid = int.Parse(id);
            await NoteDataManager.DeleteNoteFile(_db, intid);
        }

        [HttpPut]
        public async Task Put(NoteFile edited)
        {
            NoteFile live = await _db.NoteFile.FindAsync(edited.Id);

            live.LastEdited = DateTime.Now.ToUniversalTime();
            live.NoteFileName = edited.NoteFileName;
            live.NoteFileTitle = edited.NoteFileTitle;
            live.OwnerId = edited.OwnerId;

            _db.Update(live);
            await _db.SaveChangesAsync();
        }
    }
}