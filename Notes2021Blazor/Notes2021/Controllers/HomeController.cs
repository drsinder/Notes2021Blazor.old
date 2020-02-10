/*--------------------------------------------------------------------------
    **
    **  Copyright © 2020, Dale Sinder
    **
    **  Name: HomeController.cs
    **
    **  Description:
    **      Notes 2021 Home Controller
    **
    **  This program is free software: you can redistribute it and/or modify
    **  it under the terms of the GNU General Public License version 3 as
    **  published by the Free Software Foundation.
    **
    **  This program is distributed in the hope that it will be useful,
    **  but WITHOUT ANY WARRANTY; without even the implied warranty of
    **  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    **  GNU General Public License version 3 for more details.
    **
    **  You should have received a copy of the GNU General Public License
    **  version 3 along with this program in file "license-gpl-3.0.txt".
    **  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------
    */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notes2021.Manager;
using Notes2021.Models;
using Notes2021Blazor.Shared;
using PusherServer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    public class HomeController : NController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string _stylePath;

        public HomeController(
            IWebHostEnvironment appEnv,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger,
            IEmailSender emailSender,
            NotesDbContext db) : base(userManager, signInManager, db)
        {
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;

            _stylePath = appEnv.ContentRootPath + "\\wwwroot\\css\\ExportCSS\\Customizable.css";
        }

        public async Task<IActionResult> Index()
        {
            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = "User" });
                await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            }

            TZone tzone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            HomePageModel myModel = new HomePageModel
            {
                Tzone = tzone,
                UpdateClock = false
            };


        beyond:

            if (_signInManager.IsSignedIn(User))
            {
                IdentityUser usr;
                try
                {

                    usr = await _userManager.GetUserAsync(User);
                }
                catch
                {
                    await _signInManager.SignOutAsync();
                    goto beyond;
                }

                if (!User.IsInRole("User"))
                    await _userManager.AddToRoleAsync(usr, "User");

                if (_userManager.Users.Count() == 1 && !User.IsInRole("Admin"))
                {
                    // Only/First user - Make an Admin!
                    await _userManager.AddToRoleAsync(usr, "Admin");
                }

                UserData aux = NoteDataManager.GetUserData(_userManager, User, _db);
                HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(aux.Pref1));
                HttpContext.Session.SetInt32("ArchiveID", Convert.ToInt32(0));
                string uName = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db);

                //Jobs job = new Jobs();
                //if (user.Pref2)
                //{
                //    myModel.UpdateClock = true;
                //    RecurringJob.AddOrUpdate(uName, () => job.UpdateHomePageTime(uName, tzone), Cron.Minutely);
                //    RecurringJob.AddOrUpdate("delete_" + uName, () => job.CleanupHomePageTime(uName), Cron.Daily);
                //}
                //else
                //{
                //    RecurringJob.RemoveIfExists(uName);
                //}

                HttpContext.Session.SetInt32("IsSearch", 0);    // clear the searching flag
                try
                {
                    // if this user has a searchView row in the DB, delete it

                    Search searchView = await NoteDataManager.GetUserSearch(_db, aux.UserId);

                    if (searchView != null)
                    {
                        _db.Search.Remove(searchView);
                        await _db.SaveChangesAsync();
                    }
                }
                catch
                {
                    // if we cannot talk to the DB, route the user to the setup directions
                    //return RedirectToAction("SetUp");
                }

                //Direct link to 3 important files

                myModel.IFiles = await _db.NoteFile
                        .Where(p => p.NoteFileName == "announce" || p.NoteFileName == "pbnotes" || p.NoteFileName == "noteshelp")
                        .OrderBy(p => p.NoteFileName)
                        .ToListAsync();

                // History files

                myModel.HFiles = await _db.NoteFile
                    .Where(p => p.NoteFileName == "Gnotes" || p.NoteFileName == "Opbnotes")
                    .OrderBy(p => p.NoteFileName)
                    .ToListAsync();

                // Get a list of all file names for dropdown
                IEnumerable<SelectListItem> items = LocalManager.GetFileNameSelectList(_db);
                List<SelectListItem> list2 = new List<SelectListItem>();
                list2.AddRange(items);

                myModel.AFiles = list2;

                // Get a list of all file titles for dropdown
                items = LocalManager.GetFileTitleSelectList(_db);
                list2 = new List<SelectListItem>();
                list2.AddRange(items);
                myModel.ATitles = list2;

                HomePageMessage mess = await _db.HomePageMessage.OrderByDescending(p => p.Id).FirstOrDefaultAsync();
                myModel.Message = mess != null ? mess.Message : "";


            }

            ViewData["MyName"] = NoteDataManager.GetUserDisplayName(_userManager, User, _db);
            return View(_userManager, myModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public async Task<IActionResult> SetTimeZone()
        {
            TimeZoneModel model = new TimeZoneModel();
            UserData user = await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User));
            model.TimeZoneID = user.TimeZoneID;
            if (model.TimeZoneID < 1)
                model.TimeZoneID = Globals.TimeZoneDefaultID;

            List<TZone> tzones = await _db.TZone
                .OrderBy(p => p.OffsetHours)
                .ThenBy(p => p.OffsetMinutes)
                .ThenBy(p => p.Name)
                .ToListAsync();

            model.timeZone = await _db.TZone.SingleAsync(p => p.Id == model.TimeZoneID);

            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "0",
                    Text = "-- Select a Time Zone --"
                }
            };
            foreach (TZone t in tzones)
            {
                list2.Add(new SelectListItem
                {
                    Selected = t.Id == model.TimeZoneID,
                    Value = "" + t.Id,
                    Text = t.Offset + " - " + t.Name
                });
            }

            ViewBag.OptionList = list2;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetTimeZone(TimeZoneModel model)
        {
            UserData user = await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User));
            user.TimeZoneID = model.TimeZoneID;
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("TZone");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Preferences()
        {
            UserData user = await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User));

            TextReader sr = new StreamReader(_stylePath);
            ViewBag.DefaultStyle = await sr.ReadToEndAsync();
            sr.Close();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preferences(UserData model)
        {
            UserData user = await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User));

            user.Pref1 = model.Pref1;
            user.Pref2 = model.Pref2;
            user.MyStyle = model.MyStyle;

            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("MyStyle");

            return RedirectToAction("Index");
        }

        public IActionResult SampleImages()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["StartupTimeDate"] = "";
            ViewData["Alive"] = "";
            ViewData["Stats"] = "";
            if (_signInManager.IsSignedIn(User))
            {
                TZone tzone = LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db)
                    .GetAwaiter().GetResult();

                ViewData["StartupTimeDate"] = "Startup Time " +
                   tzone.Local(Globals.StartupDateTime).ToShortTimeString()
                   + " " + tzone.Local(Globals.StartupDateTime).ToShortDateString() +
                   " " + tzone.Abbreviation;

                TimeSpan timeSpan = Globals.Uptime();

                ViewData["Alive"] = "System up: " + timeSpan.Days + " Days "
                    + timeSpan.Hours + " Hours " + timeSpan.Minutes + " Minutes "
                    + timeSpan.Seconds + " Seconds";

                long count = _db.NoteHeader.Count();
                long basenotes = _db.NoteHeader
                    .Count(p => p.ResponseOrdinal == 0);

                ViewData["Stats"] = "Base Notes: " + basenotes + ", Responses: " + (count - basenotes) +
                                    ", Total Notes: " + count;
            }
            ViewData["Message"] = "About Notes 2021 : " + ViewData["StartupTimeDate"];

            return View();
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Chat(string id, bool id2 = false)
        {
            var options = new PusherOptions
            {
                Encrypted = true,
                Cluster = Globals.PusherCluster
            };
            var pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);

            if (id2)
            {
                var data = new { username = id, message = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db) + " wants to chat with you." };
                await pusher.TriggerAsync("presence-channel", "chat_request_event", data);
            }
            else
            {
                var data = new { username = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db), message = "<<HAS ARRIVED TO CHAT>>" };
                await pusher.TriggerAsync("private-notes-chat-" + id, "show_chat_message_event", data);
            }


            ViewData["ChatFrom"] = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db);  // me
            ViewData["ChatTo"] = id;                    // chat target
            return View();
        }

        public IActionResult Schema()
        {
            return View();
        }

        public IActionResult Users()
        {
            ViewData["Message"] = "User List";

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SystemMessage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SystemMessage(SystemMessage x)
        {
            var options = new PusherOptions
            {
                Encrypted = true,
                Cluster = Globals.PusherCluster

            };
            var pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);

            await pusher.TriggerAsync("notes-channel", "import_status_message_event", new { x.message });

            return View("Index");
        }


        [HttpPost]
        public ActionResult PusherAuth(string channelNameX, string socketIdX)
        {
            Request.Form.TryGetValue("socket_id", out var socketId);
            Request.Form.TryGetValue("channel_name", out var channelNames);

            //if (channelName == null)
            //    return new OkResult();

            var channelName = channelNames[0];
            {
                var options = new PusherOptions
                {
                    Encrypted = true,
                    Cluster = Globals.PusherCluster
                };
                var pusher = new Pusher(Globals.PusherAppId, Globals.PusherKey, Globals.PusherSecret, options);

                if (!_signInManager.IsSignedIn(User))
                    return new UnauthorizedResult();

                if (channelName.StartsWith("private-"))
                {
                    var auth = pusher.Authenticate(channelName, socketId);
                    var json = auth.ToJson();
                    return new ContentResult { Content = json, ContentType = "application/json" };
                }

                //else if (channelName.StartsWith("presence-"))
                {
                    string prefix = Request.IsHttps ? "https://" : "http://";
                    var channelData = new PresenceChannelData()
                    {
                        user_id = _userManager.GetUserId(User),
                        user_info = new
                        {
                            name = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db),
                            host_name = Environment.MachineName,
                            base_url = prefix + Request.Host.Value
                        }
                    };

                    var auth = pusher.Authenticate(channelName, socketId, channelData);
                    var json = auth.ToJson();
                    return new ContentResult { Content = json, ContentType = "application/json" };
                }
            }

            // should never happen
            //return new OkResult();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class TimeZoneModel
    {
        public int TimeZoneID { get; set; }

        public TZone timeZone { get; set; }

        // ReSharper disable once UnusedMember.Global
        public List<SelectListItem> Items { get; set; }
    }

    public class HomePageModel
    {
        [Required]
        [StringLength(20)]
        public string FileName { get; set; }

        //public int FileNum { get; set; }

        public List<SelectListItem> AFiles { get; set; }

        public List<SelectListItem> ATitles { get; set; }

        public TZone Tzone { get; set; }

        public string Message { get; set; }

        public bool UpdateClock { get; set; }

        // ReSharper disable once InconsistentNaming
        public List<NoteFile> IFiles { get; set; }

        public List<NoteFile> HFiles { get; set; }
    }
}
