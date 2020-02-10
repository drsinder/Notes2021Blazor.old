using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notes2021.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiNotesController : ControllerBase
    {

        private readonly NotesDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApiNotesController(NotesDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// GET
        /// Gets the list of files available.
        /// </summary>
        /// <returns>IEnumerable&lt;NoteFile&gt;&gt;
        /// List of notefile objects</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteFile>>> GetFileList()
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);

            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return new List<NoteFile>();

            List<NoteFile> allFiles = await NoteDataManager
                .GetNoteFilesOrderedByName(_context);

            List<NoteFile> myFiles = new List<NoteFile>();
            foreach (var file in allFiles)
            {
                file.Owner = null;
                NoteAccess myAccess = await AccessManager
                    .GetAccess(_context, me.Id, file.Id, 0);
                if (myAccess.ReadAccess || myAccess.Write)
                    myFiles.Add(file);
            }

            return myFiles;
        }


        /// <summary>
        /// Gets the base note headers for a file. -
        /// Used to display list of base notes.
        /// </summary>
        /// <param name="id">Id of the file you want to look at</param>
        /// <returns>IEnumerable&lt;NoteHeader&gt;&gt;
        /// List of base note headers</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<NoteHeader>>>
            GetBaseNoteHeadersForFile(int id)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);

            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return new List<NoteHeader>();
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager
                .GetAccess(_context, userID, id, 0);
            if (!myAcc.ReadAccess)
                return new List<NoteHeader>();

            List<NoteHeader> stuff = await NoteDataManager
                .GetBaseNoteHeaders(_context, id, 0);
            return stuff;
        }

        /// <summary>
        /// GET
        /// Gets the base note headers (base note and responses)
        /// for a given note in a file
        /// </summary>
        /// <param name="id">File Id to look in</param>
        /// <param name="id2">NoteOrdinal of base note (Note #)</param>
        /// <returns>IEnumerable&lt;NoteHeader&gt;&gt;</returns>
        [HttpGet("{id}/{id2}")]
        public async Task<ActionResult<IEnumerable<NoteHeader>>>
            GetNoteForFile(int id, int id2)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager
                .FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);

            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return new List<NoteHeader>();
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager
                .GetAccess(_context, userID, id, 0);
            if (!myAcc.ReadAccess)
                return new List<NoteHeader>();

            List<NoteHeader> result = await NoteDataManager
                .GetBaseNoteAndResponsesHeaders(_context, id, 0, id2);
            return result;
        }

        /// <summary>
        /// GET
        /// Gets the NoteContent part of a note.
        /// </summary>
        /// <param name="id">File Id</param>
        /// <param name="id2">NoteOrdinal</param>
        /// <param name="id3">ResponseOrdinal (0) for the base note...</param>
        /// <returns>NoteContent object</returns>
        [HttpGet("{id}/{id2}/{id3}")]
        public async Task<ActionResult<NoteContent>>
            GetNoteContent(int id, int id2, int id3)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager
                .FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);
            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return new NoteContent();
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager.GetAccess(_context, userID, id, 0);
            if (!myAcc.ReadAccess)
                return new NoteContent();

            NoteContent result = await NoteDataManager
                .GetNoteContent(_context, id, 0, id2, id3);
            return result;
        }

        /// <summary>
        /// GET
        /// Gets the tags for note.
        /// </summary>
        /// <param name="id">File Id</param>
        /// <param name="id2">NoteOrdinal</param>
        /// <param name="id3">ResponseOrdinal (0) for the base note...</param>
        /// <param name="id4">ignored dummy parameter</param>
        /// <returns>IEnumerable&lt;Tags&gt;&gt;</returns>
        [HttpGet("{id}/{id2}/{id3}/{id4}")]
        public async Task<ActionResult<IEnumerable<Tags>>>
        GetNoteTags(int id, int id2, int id3, int id4)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);
            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return new List<Tags>();
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager.GetAccess(_context, userID, id, 0);
            if (!myAcc.ReadAccess)
                return new List<Tags>();

            return await NoteDataManager.GetNoteTags(_context, id, 0, id2, id3, id4);
        }

        /// <summary>
        /// DELETE
        /// Deletes a note
        /// </summary>
        /// <param name="id">NoteHeader.Id of the note to delete</param>
        /// <returns>void</returns>
        [HttpDelete("{id}")]
        public async Task DeleteNote(long id)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);
            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return;

            NoteHeader myNote = await NoteDataManager.GetNoteById(_context, id);

            if (myNote.AuthorID != auths[1])  // user must be the Author
                return;

            await NoteDataManager.DeleteNote(_context, myNote);
        }

    }
}