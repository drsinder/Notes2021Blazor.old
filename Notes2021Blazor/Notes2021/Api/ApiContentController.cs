using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021.Controllers;
using Notes2021Blazor.Shared;
using Notes2021.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Notes2021.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiContentController : ControllerBase
    {
        private readonly NotesDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IWebHostEnvironment _appEnv;

        public ApiContentController(NotesDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IWebHostEnvironment appEnv
        )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _appEnv = appEnv;
        }

        /// <summary>
        /// POST
        /// Creates a new note
        /// </summary>
        /// <param name="inputModel">A filled out TextViewModel.
        /// If BaseNoteHeaderID is 0 a new base note is create.
        /// Otherwise BaseNoteHeaderID should be the Id of the
        /// base note to which this note will be created as a response.
        /// </param>
        /// <returns></returns>
        // POST: api/
        [HttpPost]
        public async Task<NoteHeader> Create(Notes2021.Models.TextViewModel inputModel)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);

            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return null;
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager.
                GetAccess(_context, userID, inputModel.NoteFileID, 0);
            if (!myAcc.Write)
                return null;

            await _signInManager.SignInAsync(me, false);

            DateTime now = DateTime.Now.ToUniversalTime();

            NoteHeader nheader = new NoteHeader()
            {
                LastEdited = now,
                ThreadLastEdited = now,
                CreateDate = now,
                NoteFileId = inputModel.NoteFileID,
                AuthorName = appMe.DisplayName,
                AuthorID = _userManager.GetUserId(User),
                NoteSubject = inputModel.MySubject,
                ResponseOrdinal = 0,
                ResponseCount = 0
            };

            if (inputModel.BaseNoteHeaderID == 0)
            {
                return await NoteDataManager
                    .CreateNote(_context, _userManager, nheader,
                        inputModel.MyNote.Replace("\n", "<br />"),
                        inputModel.TagLine,
                    inputModel.DirectorMessage, true, false);
            }
            NoteHeader bnh = await NoteDataManager.GetNoteHeader(_context, inputModel.BaseNoteHeaderID);
            nheader.BaseNoteId = bnh.Id;
            return await NoteDataManager.CreateResponse(_context, _userManager,
                nheader, inputModel.MyNote, inputModel.TagLine,
                inputModel.DirectorMessage, true, false);
        }

        /// <summary>
        /// PUT
        /// Edits the elements of an existing note.
        /// </summary>
        /// <param name="inputModel">A filled out TextViewModel
        /// for the note being edited.</param>
        /// <returns>NoteHeader for the edited note.</returns>
        // PUT: api/
        [HttpPut]
        public async Task<NoteHeader> Edit(Notes2021.Models.TextViewModel inputModel)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);
            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return null;
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager.GetAccess(_context, userID, inputModel.NoteFileID, 0); //TODO
            if (!myAcc.Write)
                return null;

            await _signInManager.SignInAsync(me, false);

            DateTime now = DateTime.Now.ToUniversalTime();

            NoteHeader oheader = await NoteDataManager
                .GetBaseNoteHeaderById(_context, inputModel.NoteID);

            if (oheader.AuthorID != userID) // must be a note this user wrote.
                return null;

            oheader.LastEdited = now;
            oheader.ThreadLastEdited = now;
            oheader.NoteSubject = inputModel.MySubject;

            NoteContent oContent = await NoteDataManager
                .GetNoteContent(_context, oheader.NoteFileId, 0, oheader.NoteOrdinal, oheader.ResponseOrdinal); //TODO
            oContent.NoteBody = inputModel.MyNote;
            oContent.DirectorMessage = inputModel.DirectorMessage;

            return await NoteDataManager
                .EditNote(_context, _userManager, oheader, oContent, inputModel.TagLine);
        }

        [HttpGet("{id}/{id2}")]
        public async Task<MemoryStream> GetFileAs(int id, bool id2)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');
            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);

            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return null;
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager.GetAccess(_context, userID, id, 0);  //TODO
            if (!myAcc.ReadAccess)
                return null;

            ExportController myExp = new ExportController(_appEnv,
                _userManager, _signInManager, _context);
            Notes2021.Models.ExportViewModel model = new Notes2021.Models.ExportViewModel();

            NoteFile nf = await NoteDataManager.GetFileById(_context, id);
            model.NoteOrdinal = 0;
            model.FileName = nf.NoteFileName;
            model.FileNum = nf.Id;
            model.directOutput = true;
            model.isCollapsible = id2;
            model.isHtml = id2;

            IdentityUser applicationUser = await _userManager
                .FindByEmailAsync(auths[0]);

            await _signInManager.SignInAsync(applicationUser, false);

            model.tzone = _context.TZone.Single(p => p.Id == appMe.TimeZoneID);

            return await myExp.DoExport(model, User, 0);   //TODO
        }

    }
}
