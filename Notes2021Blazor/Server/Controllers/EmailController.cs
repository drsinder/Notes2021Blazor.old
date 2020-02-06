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
using Newtonsoft.Json;
using System.IO;

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