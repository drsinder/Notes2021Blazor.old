using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notes2021.Api
{

    public class LinkCreateModel
    {
        public NoteHeader header { get; set; }

        public NoteContent content { get; set; }

        public List<Tags> tags { get; set; }

        public string linkedfile { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class ApiLinkController : ControllerBase
    {
        private readonly NotesDbContext _context;

        public ApiLinkController(NotesDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<string> CreateLinkNote(LinkCreateModel inputModel)
        {
            NoteFile file = await _context.NoteFile
                .SingleAsync(p => p.NoteFileName == inputModel.linkedfile);
            if (file == null)
                return "Target file does not exist";

            // check for acceptance

            if (!await AccessManager.TestLinkAccess(_context, file, ""))
                return "Access Denied";

            inputModel.header.NoteFileId = file.Id;
            inputModel.header.ArchiveId = 0;
            inputModel.header.BaseNoteId = 0;
            inputModel.header.Id = 0;
            inputModel.header.NoteContent = null;
            inputModel.header.NoteFile = null;
            inputModel.header.NoteOrdinal = 0;
            inputModel.header.ResponseOrdinal = 0;
            inputModel.header.ResponseCount = 0;

            var tags = Tags.ListToString(inputModel.tags);

            NoteHeader nh = await NoteDataManager.CreateNote(_context, null, inputModel.header,
                inputModel.content.NoteBody, tags, inputModel.content.DirectorMessage, true, true);

            if (nh == null)
            {

                return "Remote note create failed";
            }

            LinkLog ll = new LinkLog()
            {
                Event = "Ok",
                EventTime = DateTime.UtcNow,
                EventType = "RcvdCreateBaseNote"
            };

            _context.LinkLog.Add(ll);
            await _context.SaveChangesAsync();

            return "Ok";
        }

        [HttpGet]
#pragma warning disable 1998
        public async Task<string> Test()
#pragma warning restore 1998
        {
            return "Hello Notes2020";
        }
    }
}