using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Shared;

namespace Notes2021Blazor.Server.Controllers
{
    public class LinkCreateModel
    {
        public NoteHeader header { get; set; }

        public NoteContent content { get; set; }

        public List<Tags> tags { get; set; }

        public string linkedfile { get; set; }
        public string Secret { get; set; }
    }

    public class LinkCreateEModel
    {
        public NoteHeader header { get; set; }

        public NoteContent content { get; set; }

        public string tags { get; set; }

        public string linkedfile { get; set; }

        public string myGuid { get; set; }
        public string Secret { get; set; }
    }

    public class LinkCreateRModel
    {
        public NoteHeader header { get; set; }

        public NoteContent content { get; set; }

        public List<Tags> tags { get; set; }

        public string linkedfile { get; set; }

        public string baseGuid { get; set; }
        public string Secret { get; set; }

    }

    /// <summary>
    /// Has functions from former ApiLink, ApiLinkD, ApiLinkE Controllers
    /// </summary>

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
            NoteFile file = _context.NoteFile.SingleOrDefault(p => p.NoteFileName == inputModel.linkedfile);
            
            if (file == null)
                return "Target file does not exist";

            // check for acceptance

            if (!await AccessManager.TestLinkAccess(_context, file, inputModel.Secret))
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

        [HttpPut]
        public async Task<string> EditLinkResponse(LinkCreateEModel inputModel)
        {
            NoteFile file = _context.NoteFile.SingleOrDefault(p => p.NoteFileName == inputModel.linkedfile);
            if (file == null)
                return "Target file does not exist";

            // check for acceptance

            if (!await AccessManager.TestLinkAccess(_context, file, inputModel.Secret))
                return "Access Denied";

            // find local base note for this and modify header

            NoteHeader extant = _context.NoteHeader.SingleOrDefault(p => p.LinkGuid == inputModel.myGuid);

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

        [HttpDelete]
        public async Task<string> DeleteLinkNote(string guid)
        {
            try
            {

                NoteHeader nh = _context.NoteHeader.SingleOrDefault(p => p.LinkGuid == guid);

                if (nh == null || nh.LinkGuid != guid)
                    return "No note to delete";

                try
                {
                    // check for acceptance

                    NoteFile file = _context.NoteFile.SingleOrDefault(p => p.Id == nh.NoteFileId);
                    if (file == null)
                        return "Target file does not exist";

                    if (!await AccessManager.TestLinkAccess( _context, file, string.Empty))
                        return "Access Denied";
                }
                catch
                { // ignore
                }

                await NoteDataManager.DeleteNote(_context, nh);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Ok";
        }


        [HttpGet]
        public async Task<string> Test()
        {
            return "Hello Notes2021";
        }
    }
}
