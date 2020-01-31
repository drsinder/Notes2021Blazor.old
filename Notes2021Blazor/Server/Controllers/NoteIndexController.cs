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
    [Route("api/[controller]")]
    [Route("api/[controller]/{sid}")]
    [ApiController]
    public class NoteIndexController : ControllerBase
    {
        private readonly NotesDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public NoteIndexController(NotesDbContext db,
                UserManager<IdentityUser> userManager
                )
        {
            _db = db;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<NoteDisplayIndexModel> Get(string sid)
        {
            int id = int.Parse(sid);

            int arcId = 0;  //TODO

            bool isAdmin = User.IsInRole("Admin");

            NoteDisplayIndexModel idxModel = new NoteDisplayIndexModel();

            idxModel.linkedText = string.Empty;

            idxModel.myAccess = await GetMyAccess(id, arcId);
            if (isAdmin)
            {
                idxModel.myAccess.ViewAccess = true;
            }
            idxModel.noteFile = await NoteDataManager.GetFileById(_db, id);

            if (!idxModel.myAccess.ReadAccess && !idxModel.myAccess.Write)
            {
                idxModel.message = "You do not have access to file " + idxModel.noteFile.NoteFileName;
                return idxModel;
            }

            List<LinkedFile> linklist = await _db.LinkedFile.Where(p => p.HomeFileId == id).ToListAsync();
            if (linklist != null && linklist.Count > 0)
                idxModel.linkedText = " (Linked)";

            // Get the Base Notes Objects
            idxModel.Notes = await NoteDataManager.GetBaseNoteHeaders(_db, id, arcId);

            idxModel.AllNotes = await NoteDataManager.GetAllHeaders(_db, id, arcId);

            //idxModel.ExpandOrdinal = 0;

            idxModel.tZone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _db);
            Mark mark = await _db.Mark.Where(p => p.UserId == _userManager.GetUserId(User)).FirstOrDefaultAsync();
            idxModel.isMarked = (mark != null);

            //idxModel.rPath = Request.PathBase;

            idxModel.ArcId = arcId;

            return idxModel;
        }



        /// <summary>
        /// Get Access Control Object for file and user
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public async Task<NoteAccess> GetMyAccess(int fileid, int ArcId)
        {
            string myname = User.Identity.Name;
            var IdUser = await _userManager.FindByEmailAsync(myname);
            string uid = await _userManager.GetUserIdAsync(IdUser);

            NoteAccess noteAccess = await AccessManager.GetAccess(_db, uid, fileid, ArcId);
            return noteAccess;
        }

    }
}