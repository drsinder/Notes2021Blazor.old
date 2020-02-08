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
            NoteDisplayIndexModel idxModel = new NoteDisplayIndexModel();
            if (!sid.Contains("."))
            {
                idxModel.message = "string passed to server must be in form fileId.ArciveID";
                return idxModel;
            }

            string[] stuff = sid.Split(".");

            int id = int.Parse(stuff[0]);

            int arcId = int.Parse(stuff[1]);

            bool isAdmin = User.IsInRole("Admin");

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

            idxModel.AllNotes = await NoteDataManager.GetAllHeaders(_db, id, arcId);

            // Extract the Base Notes Objects on the client side from AllNotes

            //idxModel.ExpandOrdinal = 0;

            idxModel.tZone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _db);
            string myname = User.Identity.Name;
            var IdUser = await _userManager.FindByEmailAsync(myname);
            string uid = await _userManager.GetUserIdAsync(IdUser);

            List<Mark> marks = await _db.Mark.Where(p => p.UserId == uid).ToListAsync();
            idxModel.isMarked = (marks != null && marks.Count > 0);
            if (idxModel.isMarked)
            {
                idxModel.isMarked = marks.Where(p => p.NoteFileId == id && p.ArchiveId == arcId).Any();
            }

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