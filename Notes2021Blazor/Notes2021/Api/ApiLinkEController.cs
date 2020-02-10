using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System.Threading.Tasks;

namespace Notes2021.Api
{
    public class LinkCreateEModel
    {
        public NoteHeader header { get; set; }

        public NoteContent content { get; set; }

        public string tags { get; set; }

        public string linkedfile { get; set; }

        public string myGuid { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ApiLinkEController : ControllerBase
    {
        private readonly NotesDbContext _context;

        public ApiLinkEController(NotesDbContext context)
        {
            _context = context;
        }

        [HttpPut]
        public async Task<string> EditLinkResponse(LinkCreateEModel inputModel)
        {
            NoteFile file = await _context.NoteFile.SingleAsync(p => p.NoteFileName == inputModel.linkedfile);
            if (file == null)
                return "Target file does not exist";

            // check for acceptance

            if (!await AccessManager.TestLinkAccess( _context, file, ""))
                return "Access Denied";

            // find local base note for this and modify header

            NoteHeader extant = await _context.NoteHeader.SingleAsync(p => p.LinkGuid == inputModel.myGuid);

            if (extant == null) // || extant.NoteFileId != file.Id)
                return "Could not find note";

            inputModel.header.NoteOrdinal = extant.NoteOrdinal;
            inputModel.header.ArchiveId = extant.ArchiveId;

            inputModel.header.NoteFileId = file.Id;

            inputModel.header.BaseNoteId = extant.BaseNoteId;
            inputModel.header.Id = extant.Id;
            //inputModel.header.NoteContent = null;
            //inputModel.header.NoteFile = null;
            inputModel.header.ResponseOrdinal = extant.ResponseOrdinal;
            inputModel.header.ResponseCount = extant.ResponseCount;


            NoteHeader nh = await NoteDataManager.EditNote(_context, null, inputModel.header,
                inputModel.content, inputModel.tags);

            if (nh == null)
            {

                return "Remote response edit failed";
            }

            return "Ok";
        }


    }
}