using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Server.Services;
using Notes2021Blazor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
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

            await ProcessLinkedNotes();

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

            await ProcessLinkedNotes();

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

            await ProcessLinkedNotes();
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

            string myEmail = await LocalService.MakeNoteForEmail(fv, _db, Globals.PrimeAdminEmail, Globals.PrimeAdminName);

            EmailSender emailSender = new EmailSender();

            foreach (Subscription s in subs)
            {
                IdentityUser usr = await _userManager.FindByIdAsync(s.SubscriberId);
                await emailSender.SendEmailAsync(usr.UserName, myNote.NoteSubject, myEmail);
            }
        }

        private async Task ProcessLinkedNotes()
        {
            List<LinkQueue> items = await _db.LinkQueue.Where(p => p.Enqueued == false).ToListAsync();
            foreach (LinkQueue item in items)
            {
                LinkProcessor lp = new LinkProcessor(_db);
                BackgroundJob.Enqueue(() => lp.ProcessLinkAction(item.Id));
                item.Enqueued = true;
                _db.Update(item);
            }
            if (items.Count > 0)
                await _db.SaveChangesAsync();

        }

    }
}
