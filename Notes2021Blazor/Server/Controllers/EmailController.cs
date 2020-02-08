using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Server.Services;
using Notes2021Blazor.Shared;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Controllers
{
    [Authorize(Roles = "User")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly NotesDbContext _db;

        public EmailController(NotesDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task Post(EmailModel stuff)
        {
            EmailSender sender = new EmailSender();

            await sender.SendEmailAsync(stuff.email, stuff.subject, stuff.payload);
        }

    }
}