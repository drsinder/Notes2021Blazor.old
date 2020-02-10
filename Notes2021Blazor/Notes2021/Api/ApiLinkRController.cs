using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notes2021.Api
{
    public class LinkCreateRModel
    {
        public NoteHeader header { get; set; }

        public NoteContent content { get; set; }

        public List<Tags> tags { get; set; }

        public string linkedfile { get; set; }

        public string baseGuid { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class ApiLinkRController : ControllerBase
    {
        private readonly NotesDbContext _context;

        public ApiLinkRController(NotesDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<string> CreateLinkResponse(LinkCreateRModel inputModel)
        {
            NoteFile file = await _context.NoteFile.SingleAsync(p => p.NoteFileName == inputModel.linkedfile);
            if (file == null)
                return "Target file does not exist";

            // check for acceptance

            if (!await AccessManager.TestLinkAccess(_context, file, ""))
                return "Access Denied";

            // find local base note for this and modify header

            NoteHeader extant = await _context.NoteHeader.SingleAsync(p => p.LinkGuid == inputModel.baseGuid);

            if (extant == null) // || extant.NoteFileId != file.Id)
                return "Could not find base note";

            inputModel.header.NoteOrdinal = extant.NoteOrdinal;

            inputModel.header.NoteFileId = file.Id;

            inputModel.header.BaseNoteId = extant.BaseNoteId;
            inputModel.header.Id = 0;
            inputModel.header.NoteContent = null;
            inputModel.header.NoteFile = null;
            //inputModel.header.ResponseOrdinal = 0;
            //inputModel.header.ResponseCount = 0;

            var tags = Tags.ListToString(inputModel.tags);

            NoteHeader nh = await NoteDataManager.CreateResponse(_context, null, inputModel.header,
                inputModel.content.NoteBody, tags, inputModel.content.DirectorMessage, true, true);

            if (nh == null)
            {

                return "Remote response create failed";
            }

            return "Ok";
        }
    }
}