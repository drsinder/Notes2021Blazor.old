using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]/{fileId}")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccessListController : ControllerBase
    {
        private readonly NotesDbContext _db;

        public AccessListController(NotesDbContext db)
        {
            _db = db;

        }

        [HttpGet]
        public async Task<List<NoteAccess>> Get(string fileId)
        {
            int Id = int.Parse(fileId);

            List<NoteAccess> list = await _db.NoteAccess.Where(p => p.NoteFileId == Id).OrderBy(p => p.ArchiveId).ToListAsync();

            return list;
        }

        [HttpPut]
        public async Task Put(NoteAccess item)
        {
            NoteAccess work = await _db.NoteAccess.Where(p => p.NoteFileId == item.NoteFileId
                && p.ArchiveId == item.ArchiveId && p.UserID == item.UserID)
                .FirstOrDefaultAsync();
            if (work == null)
                return;

            work.ReadAccess = item.ReadAccess;
            work.Respond = item.Respond;
            work.Write = item.Write;
            work.DeleteEdit = item.DeleteEdit;
            work.SetTag = item.SetTag;
            work.ViewAccess = item.ViewAccess;
            work.EditAccess = item.EditAccess;

            _db.Update(work);
            await _db.SaveChangesAsync();
        }

    }
}