using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Server.Services;
using Notes2021Blazor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            NoteFile nf = _db.NoteFile.Single(p => p.Id == Id);

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

            await SendNewNoteToSubscribers(created);
            
            return created;
        }


        [HttpPut]
        public async Task Put(TextViewModel tvm)
        {
            if (tvm.MyNote == null)
                return;

            //UserData me = NoteDataManager.GetUserData(_userManager, User, _db);

            DateTime now = DateTime.Now.ToUniversalTime();

            tvm.NoteHeader.NoteSubject = tvm.MySubject;
            tvm.NoteHeader.LastEdited = now;
            tvm.NoteHeader.ThreadLastEdited = now;

            NoteContent nc = new NoteContent
            {
                NoteHeaderId = tvm.NoteHeader.Id,
                NoteBody = tvm.MyNote,
                DirectorMessage = tvm.DirectorMessage
            };

            await NoteDataManager.EditNote(_db, _userManager, tvm.NoteHeader, nc, tvm.TagLine);

            return;
        }

        [HttpDelete]
        public async Task Delete(string fileId)
        {
            long noteId = long.Parse(fileId);

            NoteHeader nh = _db.NoteHeader.SingleOrDefault(p => p.Id == noteId);
            if (nh == null)
                return;

            await NoteDataManager.DeleteNote(_db, nh);
        }


        private async Task SendNewNoteToSubscribers(NoteHeader myNote)
        {
            List<Subscription> subs = await _db.Subscription
                .Where(p => p.NoteFileId == myNote.NoteFileId)
                .ToListAsync();

            if (subs == null || subs.Count == 0)
                return;

            ForwardViewModel fv = new ForwardViewModel();
            fv.NoteID = myNote.Id;

            string myEmail = await MakeNoteForEmail(fv, _db, Globals.PrimeAdminEmail, Globals.PrimeAdminName);

            EmailSender emailSender = new EmailSender();

            foreach (Subscription s in subs)
            {
                IdentityUser usr = await _userManager.FindByIdAsync(s.SubscriberId);
                await emailSender.SendEmailAsync(usr.UserName, myNote.NoteSubject, myEmail);
            }
        }

        private static async Task<string> MakeNoteForEmail(ForwardViewModel fv, NotesDbContext db, string email, string name)
        {
            NoteHeader nc = await NoteDataManager.GetNoteByIdWithFile(db, fv.NoteID);

            if (!fv.hasstring || !fv.wholestring)
            {
                return "Forwarded by Notes 2021 - User: " + email + " / " + name
                    + "<p>File: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p><hr/>"
                    + "<p>Author: " + nc.AuthorName + "  - Director Message: " + nc.NoteContent.DirectorMessage + "</p><p>"
                    + "<p>Subject: " + nc.NoteSubject + "</p>"
                    + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UTC" + "</p>"
                    + nc.NoteContent.NoteBody
                    + "<hr/>" + "<a href=\"" + Globals.ProductionUrl + "NoteDisplay/Display/" + fv.NoteID + "\" >Link to note</a>";
            }
            else
            {
                List<NoteHeader> bnhl = await db.NoteHeader
                    .Where(p => p.NoteFileId == nc.NoteFileId && p.NoteOrdinal == nc.NoteOrdinal && p.ResponseOrdinal == 0)
                    .ToListAsync();
                NoteHeader bnh = bnhl[0];
                fv.NoteSubject = bnh.NoteSubject;
                List<NoteHeader> notes = await db.NoteHeader.Include("NoteContent")
                    .Where(p => p.NoteFileId == nc.NoteFileId && p.NoteOrdinal == nc.NoteOrdinal)
                    .ToListAsync();

                StringBuilder sb = new StringBuilder();
                sb.Append("Forwarded by Notes 2020 - User: " + email + " / " + name
                    + "<p>\nFile: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p>"
                    + "<hr/>");

                for (int i = 0; i < notes.Count; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("<p>Base Note - " + (notes.Count - 1) + " Response(s)</p>");
                    }
                    else
                    {
                        sb.Append("<hr/><p>Response - " + notes[i].ResponseOrdinal + " of " + (notes.Count - 1) + "</p>");
                    }
                    sb.Append("<p>Author: " + notes[i].AuthorName + "  - Director Message: " + notes[i].NoteContent.DirectorMessage + "</p>");
                    sb.Append("<p>Subject: " + notes[i].NoteSubject + "</p>");
                    sb.Append("<p>" + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UTC" + " </p>");
                    sb.Append(notes[i].NoteContent.NoteBody);
                    sb.Append("<hr/>");
                    sb.Append("<a href=\"");
                    sb.Append(Globals.ProductionUrl + "NoteDisplay/Display/" + notes[i].Id + "\" >Link to note</a>");
                }

                return sb.ToString();
            }

            return string.Empty;
        }

    }
}
