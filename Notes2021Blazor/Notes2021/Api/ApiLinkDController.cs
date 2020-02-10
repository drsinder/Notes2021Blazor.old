using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System;
using System.Threading.Tasks;

namespace Notes2021.Api
{

    public class LinkDeleteModel
    {
        public string baseGuid { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ApiLinkDController : ControllerBase
    {
        private readonly NotesDbContext _context;

        public ApiLinkDController(NotesDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<string> DeleteLinkNote(LinkDeleteModel guid)
        {
            try
            {

                NoteHeader nh = await _context.NoteHeader.SingleOrDefaultAsync(p => p.LinkGuid == guid.baseGuid);

                if (nh == null || nh.LinkGuid != guid.baseGuid)
                    return "No note to delete";

                try
                {
                    // check for acceptance

                    NoteFile file = await _context.NoteFile.SingleAsync(p => p.Id == nh.NoteFileId);
                    if (file == null)
                        return "Target file does not exist";

                    if (!await AccessManager.TestLinkAccess(_context, file, ""))
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
    }
}