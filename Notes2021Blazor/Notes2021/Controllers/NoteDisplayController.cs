/*--------------------------------------------------------------------------
    **
    **  Copyright © 2019, Dale Sinder
    **
    **  Name: NoteDisplayController.cs
    **
    **  Description:
    **      Notes 2020 Note Display Controller
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

using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Notes2021.Api;
using Notes2021.Manager;
using Notes2021.Models;
using Notes2021Blazor.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    [Authorize(Roles = "User")]
    public class NoteDisplayController : NController
    {
        private readonly IWebHostEnvironment _appEnv;
        private readonly IEmailSender _emailSender;

        public NoteDisplayController(
            IWebHostEnvironment appEnv,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            //ISmsSender smsSender,
            NotesDbContext NotesDbContext
            ) : base(userManager, signInManager, NotesDbContext)
        {
            _emailSender = emailSender;
            //_smsSender = smsSender;
            _appEnv = appEnv;

            //_telemetry = tel;
            //_telemetry.InstrumentationKey = Globals.InstKey;

            Globals.EmailSender = _emailSender;
        }

        /// <summary>
        /// Utility to enter file based on contents of a NoteDisplayViewModel1 Object
        /// </summary>
        /// <param name="model">NFViewModel Object</param>
        /// <returns></returns>
        public async Task<IActionResult> EnterFile(NoteDisplayViewModel1 model)
        {
            NoteFile nf = await NoteDataManager.GetFileByName(_db, model.FileName);

            if (nf != null)
            {
                int id = nf.Id;
                if (_signInManager.IsSignedIn(User))
                {
                    UserData user = NoteDataManager.GetUserData(_userManager, User, _db);
                    HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(user.Pref1));
                }

                HttpContext.Session.SetInt32("ArchiveID", Convert.ToInt32(0));

                return RedirectToAction("Listing", new { id });
            }
            ViewBag.Message = "Could not find file '" + model.FileName + "'.";
            return View("Error");
        }


        /// <summary>
        /// Utility to enter a file based on a NoteFileID
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public async Task<IActionResult> EnterFileByID(int id)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, id);

            if (nf != null)
            {
                if (_signInManager.IsSignedIn(User))
                {
                    UserData user = NoteDataManager.GetUserData(_userManager, User, _db);
                    HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(user.Pref1));
                }

                HttpContext.Session.SetInt32("ArchiveID", Convert.ToInt32(0));

                return RedirectToAction("Listing", new { id });
            }
            ViewBag.Message = "Could not find fileID '" + id + "'.";
            return View("Error");
        }

        /// <summary>
        /// Utility to enter a file based on it's name
        /// </summary>
        /// <param name="id">NoteFileName</param>
        /// <returns></returns>
        public async Task<IActionResult> Enter(string id)
        {
            NoteFile noteFile = await NoteDataManager.GetFileByName(_db, id);

            if (noteFile != null)
            {
                if (_signInManager.IsSignedIn(User))
                {
                    UserData user = NoteDataManager.GetUserData(_userManager, User, _db);
                    HttpContext.Session.SetInt32("HideNoteMenu", Convert.ToInt32(user.Pref1));
                }

                HttpContext.Session.SetInt32("ArchiveID", Convert.ToInt32(0));

                return RedirectToAction("Listing", new { id = noteFile.Id });
            }
            ViewBag.Message = "Could not find file '" + id + "'.";
            return View("Error");

        }

        /// <summary>
        /// Get Access Control Object for file and user
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        public async Task<NoteAccess> GetMyAccess(int fileid, int ArcId)
        {
            NoteAccess noteAccess = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), fileid, ArcId);
            ViewData["MyAccess"] = noteAccess;
            return noteAccess;
        }

        /// <summary>
        /// Get a BaseNoteHeader Object given a NoteContent Object
        /// </summary>
        /// <param name="nc">NoteHeader Object</param>
        /// <returns></returns>
        public async Task<NoteHeader> GetBaseNoteHeader(NoteHeader nc)
        {
            if (nc.ResponseOrdinal == 0)
                return nc;

            return await NoteDataManager.GetNoteHeader(_db, nc.BaseNoteId);

            //return await _db.NoteHeader
            //    .Where(x => x.NoteFileId == nc.NoteFileId && x.NoteOrdinal == nc.NoteOrdinal && x.ResponseOrdinal == 0)
            //    .SingleAsync();
        }

        /// <summary>
        /// Set a variety of ViewData items for user by the View
        /// </summary>
        /// <param name="nc"></param>
        public async Task<NoteXModel> GetNoteExtras(NoteHeader nc)
        {
            NoteXModel model = new NoteXModel()
            {
                bnh = await GetBaseNoteHeader(nc),
                nh = nc,
                CanDelete = true,
                DeleteMessage = ""
            };
            if (model.bnh.ResponseCount > 0 && nc.ResponseOrdinal == 0)   // base note with responses
            {
                model.CanDelete = false;
                model.DeleteMessage = "You may not delete/edit this Base Note because it has response(s).";
            }
            else if (model.bnh.ResponseCount > 0 && nc.ResponseOrdinal < model.bnh.ResponseCount)  // not the last response
            {
                model.CanDelete = false;
                model.DeleteMessage = "You may not delete/edit this Response because Response(s) follow it.";
            }

            // now allow delete only by note writer
            if (model.CanDelete)
            {
                if (_userManager.GetUserId(User) != nc.AuthorID)
                {
                    model.CanDelete = false;
                    model.DeleteMessage = "You are not the writer of this Note.";
                }
            }

            if (User.IsInRole("Admin"))
            { model.CanDelete = true; }

            return model;
        }

        public IActionResult Archives(int id)
        {
            HttpContext.Session.SetInt32("IsSearch", 0);  // Mark not doing a search

            NoteFile nf = _db.NoteFile.Single(p => p.Id == id);

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            if (arcId == 0)
                arcId = nf.NumberArchives;
            else
                arcId--;

            if (arcId < 0)
                arcId = 0;

            HttpContext.Session.SetInt32("ArchiveID", Convert.ToInt32(arcId));


            return RedirectToAction("Listing", new { id = id });
        }

        /// <summary>
        /// Prepare data for display of NoteFile Index or List of Base Notes
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <param name="id2">For scrolling to the note you last displayed</param>
        /// <returns></returns>
        public async Task<IActionResult> Listing(int id, string id2)
        {
            HttpContext.Session.SetInt32("IsSearch", 0);  // Mark not doing a search

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            bool isAdmin = User.IsInRole("Admin");

            if (id < 1)
            {
                return RedirectToAction("Index", "NoteFileList");
                //ViewBag.Message = "FileID given is null.";
                //return View("Error");
            }
            Notes2021.Models.NoteDisplayIndexModel idxModel = new Notes2021.Models.NoteDisplayIndexModel();

            idxModel.linkedText = string.Empty;

            if (!string.IsNullOrEmpty(id2))
            {
                PathString myreq = Request.Path;
                Request.Path = myreq + "#" + id2;
            }

            // Check that user can read and/or write the file
            NoteAccess myacc = await GetMyAccess(id, arcId);
            if (myacc == null)
            {
                ViewBag.Message = "Could not find access for fileid '" + id + "'.";
                return View("Error");
            }

            NoteFile nf = await NoteDataManager.GetFileById(_db, id);
            idxModel.noteFile = nf;

            if (nf == null)
            {
                ViewBag.Message = "Could not find fileid '" + id + "'.";
                return View("Error");
            }
            if (!myacc.ReadAccess && !myacc.Write)
            {

                ViewBag.Message = "You do not have access to file '" + nf.NoteFileName + "'.";
                return View("Error");
            }

            List<LinkedFile> linklist = await _db.LinkedFile.Where(p => p.HomeFileId == id).ToListAsync();
            if (linklist != null && linklist.Count > 0)
                idxModel.linkedText = " (Linked)";

            // Get the Base Notes Objects
            idxModel.Notes = await NoteDataManager.GetBaseNoteHeaders(_db, id, arcId);


            if (isAdmin)
            {
                myacc.ViewAccess = true;
            }
            idxModel.myAccess = myacc;
            idxModel.ExpandOrdinal = 0;

            idxModel.tZone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);
            Mark mark = await _db.Mark.Where(p => p.UserId == _userManager.GetUserId(User)).FirstOrDefaultAsync();
            idxModel.isMarked = (mark != null);

            if (!string.IsNullOrEmpty(id2))
            {
                idxModel.scroller = "#" + id2;
            }

            idxModel.rPath = Request.PathBase;

            idxModel.ArcId = arcId;

            // Pass NoteFile list to View
            return View(_userManager, idxModel);
        }


        public async Task<IActionResult> Expand(long? id)
        {
            HttpContext.Session.SetInt32("IsSearch", 0);  // Mark not doing a search
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            if (id == null)
            {
                ViewBag.Message = "Base NoteID given is null.";
                return View("Error");
            }

            NoteHeader note = await NoteDataManager.GetNoteById(_db, (long)id);
            if (note == null)
            {
                ViewBag.Message = "Could not find note for Base NoteID '" + id + "'.";
                return View("Error");
            }

            Notes2021.Models.NoteDisplayIndexModel idxModel = new Notes2021.Models.NoteDisplayIndexModel();
            NoteFile nf = await NoteDataManager.GetFileById(_db, note.NoteFileId);
            if (nf == null)
            {
                ViewBag.Message = "Could not find fileid '" + id + "'.";
                return View("Error");
            }
            idxModel.noteFile = nf;

            // Check that user can read and/or write the file
            NoteAccess myacc = await GetMyAccess(note.NoteFileId, arcId);
            if (myacc == null)
            {
                ViewBag.Message = "Could not find access for fileid '" + id + "'.";
                return View("Error");
            }
            else if (!myacc.ReadAccess && !myacc.Write)
            {
                ViewBag.Message = "You do not have access to file '" + nf.NoteFileName + "'.";
                return View("Error");
            }

            // Get the Base Notes Objects
            idxModel.Notes = await NoteDataManager.GetBaseNoteHeaders(_db, nf.Id, arcId);

            idxModel.myAccess = myacc;
            idxModel.ExpandOrdinal = note.NoteOrdinal;

            if (idxModel.Notes == null)
            {
                ViewBag.Message = "Could not get base note '" + note.NoteOrdinal + "' and responses for '" + idxModel.noteFile.NoteFileName + "'.";
                return View("Error");
            }

            idxModel.Cheaders = new List<string>();
            idxModel.Lheaders = new List<string>();

            foreach (NoteHeader nc in idxModel.Notes)
            {
                idxModel.Cheaders.Add("<div class=\"container\"><div class=\"panel-group\">"
                        + "<div class=\"panel panel-default\"><div class=\"panel-heading\"><div class=\"panel-title\">"
                        + "</div></div><div id = \"collapse" + nc.Id
                        + "\" class=\"panel-collapse collapse\"><div class=\"panel-body\">");

                idxModel.Lheaders.Add("<a data-toggle =\"collapse\" href=\"#collapse" + nc.Id + "\">&gt;</a>");
            }

            idxModel.panelEnd = "</div></div></div></div></div>";

            Mark mark = await _db.Mark.Where(p => p.UserId == _userManager.GetUserId(User)).FirstOrDefaultAsync();
            idxModel.isMarked = (mark != null);

            idxModel.rPath = Request.PathBase;

            idxModel.tZone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            idxModel.Expanded = await NoteDataManager.GetBaseNoteAndResponses(_db, idxModel.noteFile.Id, arcId, idxModel.ExpandOrdinal);


            // Pass NoteFile list to View
            return View(_userManager, "Index", idxModel);
        }

        public IActionResult Error(string message)
        {
            ViewBag.Message = message;
            return View();
        }


        /// <summary>
        /// Display a note and present list of actions
        /// </summary>
        /// <param name="id">NoteID of Note to display</param>
        /// <returns></returns>
        public async Task<IActionResult> Display(long? id)
        {
            if (id == null || id == 0)
            {
                //return RedirectToAction("Index", "NoteFileList");
                ViewBag.Message = "NoteID given is null.";
                return View("Error");
            }

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            // Get the Note Object
            NoteHeader nc = await NoteDataManager.GetNoteByIdWithFile(_db, (long)id);
            if (nc == null)
            {
                ViewBag.Message = "NoteID '" + (long)id + "' could not be found.";
                return View("Error");
            }

            // ReSharper disable once NotAccessedVariable
            string page = "Display " + nc.NoteFile.NoteFileName + " Note " + nc.NoteOrdinal;
            if (nc.ResponseOrdinal > 0)
            {
                // ReSharper disable once RedundantAssignment
                page += "." + nc.ResponseOrdinal;
            }


            NoteXModel model = await GetNoteExtras(nc);  // set some Model Data
            model.myAccess = await GetMyAccess(nc.NoteFile.Id, arcId);

            // Check if sequencing
            if (HttpContext.Session.GetInt32("CurrentSeq") != null && HttpContext.Session.GetInt32("CurrentSeq") == 1)
                model.IsSeq = true;
            else
                model.IsSeq = false;

            model.tZone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            int? menu = HttpContext.Session.GetInt32("HideNoteMenu");
            ViewBag.HideNoteMenu = false;
            if (menu != null && menu > 0)
                ViewBag.HideNoteMenu = true;

            //byte[] b2 = HttpContext.Session.Get("MyStyle");
            //if (b2 != null)
            //    ViewBag.MyStyle = Encoding.Default.GetString(b2);

            // Send NoteXModel Object to View
            return View(_userManager, model);
        }

        // Use Fancy WYSISYG Html editor
        public void GetIsTinymce()
        {
            ViewData["isTinymce"] = true;
        }


        // GET: NoteDisplay/Create
        /// <summary>
        /// Set up for Create a new Base Note in a given FileID
        /// </summary>
        /// <param name="id">FileID</param>
        /// <returns></returns>
        public async Task<IActionResult> Create(int id)
        {
            GetIsTinymce();
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteAccess myAccess = await GetMyAccess(id, arcId);
            if (!myAccess.Write)
                return RedirectToAction("Index", "NoteFileList");

            Notes2021.Models.TextViewModel test = new Notes2021.Models.TextViewModel()
            {
                NoteFileID = id,
                BaseNoteHeaderID = 0
            };
            NoteFile nf = await NoteDataManager.GetFileById(_db, id);

            test.noteFile = nf;
            return View(test);
        }

        // GET: NoteDisplay/Create
        /// <summary>
        /// Set up for Create a new Response Note given NoteID
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> CreateResponse(long id)
        {
            GetIsTinymce();

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            Notes2021.Models.TextViewModel test = new Notes2021.Models.TextViewModel()
            {
                BaseNoteHeaderID = id
            };
            NoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, id);

            test.NoteFileID = bnh.NoteFileId;
            NoteFile nf = await NoteDataManager.GetFileById(_db, bnh.NoteFileId);

            test.noteFile = nf;

            NoteAccess myAccess = await GetMyAccess(bnh.NoteFileId, arcId);
            if (!myAccess.Write && !myAccess.Respond)
                return RedirectToAction("Index", "NoteFileList");

            return View("Create", test);
        }



        // POST: NoteDisplay/Create
        // To protect from over posting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Process Creation of new Base and Response Notes
        /// </summary>
        /// <param name="model">NoteContent from View</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        public async Task<IActionResult> Create(Notes2021.Models.TextViewModel model)
        {
            if (!ModelState.IsValid)
            {
                GetIsTinymce();
                return View("Create", model);
            }

            DateTime now = DateTime.Now.ToUniversalTime();

            UserData me = NoteDataManager.GetUserData(_userManager, User, _db);

            NoteHeader nheader = new NoteHeader()
            {
                LastEdited = now,
                ThreadLastEdited = now,
                CreateDate = now,
                NoteFileId = model.NoteFileID,
                AuthorName = me.DisplayName,
                AuthorID = me.UserId,
                NoteSubject = model.MySubject,
                ResponseOrdinal = 0,
                ResponseCount = 0
            };

            NoteHeader newNote;

            if (model.BaseNoteHeaderID == 0)
                newNote = await NoteDataManager.CreateNote(_db, _userManager, nheader, model.MyNote, model.TagLine, model.DirectorMessage, true, false);
            else
            {
                NoteHeader bnh = await NoteDataManager.GetNoteHeader(_db, model.BaseNoteHeaderID);
                nheader.BaseNoteId = bnh.Id;
                newNote = await NoteDataManager.CreateResponse(_db, _userManager, nheader, model.MyNote, model.TagLine, model.DirectorMessage, true, false);
            }

            List<LinkQueue> items = await _db.LinkQueue.Where(p => p.Enqueued == false).ToListAsync();
            foreach( LinkQueue item in items)
            {
                LinkProcessor lp = new LinkProcessor(_db);
                BackgroundJob.Enqueue(() => lp.ProcessLinkAction(item.Id));
                item.Enqueued = true;
                _db.Update(item);
            }
            if (items.Count > 0)
                await _db.SaveChangesAsync();

            //TODO
            //await SendNewNoteToSubscribers(_db, _userManager, newNote);

            return RedirectToAction("Display", new { id = newNote.Id });
        }

        //private static async Task SendNewNoteToSubscribers(NotesDbContext db, UserManager<IdentityUser> userManager, NoteHeader nc)
        //{
        //    nc.NoteFile.NoteHeaders = null;
        //    ForwardViewModel fv = new ForwardViewModel()
        //    {
        //        NoteSubject = "New Note from Notes 2021"
        //    };
        //    List<Subscription> subs = await db.Subscription
        //        .Where(p => p.NoteFileId == nc.NoteFileId)
        //        .ToListAsync();

        //    List<string> emails = new List<string>();

        //    fv.ToEmail = "xx";
        //    fv.FileID = nc.NoteFileId;
        //    fv.hasstring = false;
        //    fv.NoteID = nc.Id;
        //    fv.NoteOrdinal = nc.NoteOrdinal;
        //    fv.NoteSubject = nc.NoteSubject;
        //    fv.toAllUsers = false;
        //    fv.IsAdmin = false;
        //    fv.wholestring = false;

        //    string payload = await MakeNoteForEmail(fv, db, "BackgroundJob", "Notes 2021");

        //    foreach (Subscription s in subs)
        //    {
        //        UserData usr = await db.UserData.SingleAsync(p => p.UserId == s.SubscriberId);
        //        NoteAccess myAccess = await AccessManager.GetAccess(db, usr.UserId, nc.NoteFileId, 0);

        //        if (myAccess.ReadAccess)
        //        {
        //            //emails.Add(usr.Email);
        //        }
        //    }

        //    if (emails.Count > 0)
        //    {
        //        string payload = await MakeNoteForEmail(fv, db, "BackgroundJob", "Notes 2021");
        //        await Globals.EmailSender.SendEmailListAsync(emails, fv.NoteSubject, payload);
        //    }
        //}


        //private static async Task<string> MakeNoteForEmail(ForwardViewModel fv, NotesDbContext db, string email, string name)
        //{
        //    NoteHeader nc = await GetNoteByIdWithFile(db, fv.NoteID);

        //    if (!fv.hasstring || !fv.wholestring)
        //    {
        //        return "Forwarded by Notes 2021 - User: " + email + " / " + name
        //            + "<p>File: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p><hr/>"
        //            + "<p>Author: " + nc.AuthorName + "  - Director Message: " + nc.NoteContent.DirectorMessage + "</p><p>"
        //            + "<p>Subject: " + nc.NoteSubject + "</p>"
        //            + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UTC" + "</p>"
        //            + nc.NoteContent.NoteBody
        //            + "<hr/>" + "<a href=\"" + Globals.ProductionUrl + "NoteDisplay/Display/" + fv.NoteID + "\" >Link to note</a>";
        //    }
        //    else
        //    {
        //        List<NoteHeader> bnhl = await GetBaseNoteHeadersForNote(db, nc.NoteFileId, nc.NoteOrdinal);
        //        NoteHeader bnh = bnhl[0];
        //        fv.NoteSubject = bnh.NoteSubject;
        //        List<NoteHeader> notes = await GetBaseNoteAndResponses(db, nc.NoteFileId, nc.NoteOrdinal);

        //        StringBuilder sb = new StringBuilder();
        //        sb.Append("Forwarded by Notes 2020 - User: " + email + " / " + name
        //            + "<p>\nFile: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p>"
        //            + "<hr/>");

        //        for (int i = 0; i < notes.Count; i++)
        //        {
        //            if (i == 0)
        //            {
        //                sb.Append("<p>Base Note - " + (notes.Count - 1) + " Response(s)</p>");
        //            }
        //            else
        //            {
        //                sb.Append("<hr/><p>Response - " + notes[i].ResponseOrdinal + " of " + (notes.Count - 1) + "</p>");
        //            }
        //            sb.Append("<p>Author: " + notes[i].AuthorName + "  - Director Message: " + notes[i].NoteContent.DirectorMessage + "</p>");
        //            sb.Append("<p>Subject: " + notes[i].NoteSubject + "</p>");
        //            sb.Append("<p>" + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UTC" + " </p>");
        //            sb.Append(notes[i].NoteContent.NoteBody);
        //            sb.Append("<hr/>");
        //            sb.Append("<a href=\"");
        //            sb.Append(Globals.ProductionUrl + "NoteDisplay/Display/" + notes[i].Id + "\" >Link to note</a>");
        //        }

        //        return sb.ToString();
        //    }

        //}

        /// <summary>
        /// Given a FileID and NoteOrdinal get the Base Note NoteID
        /// </summary>
        /// <returns></returns>
        public async Task<long?> FindBaseNoteID(int fileid, int arcId, int noteord)
        {
            NoteHeader bnh = await NoteDataManager.GetBaseNoteHeaderForOrdinal(_db, fileid, arcId, noteord);

            return bnh?.Id;
        }

        // GET: NoteDisplay/Edit/5
        /// <summary>
        /// Prepare to edit a note
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NoteHeader note = await NoteDataManager.GetNoteByIdWithFile(_db, (long)id);

            if (note == null)
            {
                return NotFound();
            }

            GetIsTinymce();

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteXModel model = await GetNoteExtras(note);
            model.myAccess = await GetMyAccess(note.NoteFileId, arcId);

            if (!model.CanDelete)
            {
                ViewData["Note"] = note;
                return RedirectToAction("NoDelete", new { id });
            }

            ViewData["NoteFile"] = note.NoteFile;

            Notes2021.Models.TextViewModel test = new Notes2021.Models.TextViewModel
            {
                MyNote = note.NoteContent.NoteBody,
                MySubject = note.NoteSubject,
                NoteID = note.Id,
                noteFile = note.NoteFile,
                TagLine = string.Empty
            };

            foreach (Tags tag in note.Tags)
            {
                test.TagLine += tag.Tag + " ";
            }

            test.DirectorMessage = note.NoteContent.DirectorMessage;


            return View(test);
        }

        // POST: NoteDisplay/Edit/
        // To protect from over posting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// Process edit of a note
        /// </summary>
        /// <param name="nc">NoteContent from View</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateInput(false)]
        public async Task<IActionResult> Edit(Notes2021.Models.TextViewModel nc)
        {
            // Get copy of NoteContent from DB
            NoteHeader edited = await NoteDataManager.GetNoteById(_db, nc.NoteID);

            if (edited == null)
            {
                return NotFound();
            }

            // Copy edited values into place
            edited.LastEdited = DateTime.Now.ToUniversalTime();
            edited.NoteContent.NoteBody = nc.MyNote;
            edited.NoteSubject = nc.MySubject;

            edited.NoteContent.DirectorMessage = nc.DirectorMessage;

            // deal with tags

            _db.Tags.RemoveRange(edited.Tags);

            if (nc.TagLine != null && nc.TagLine.Length > 1)
            {
                List<Tags> theTags = new List<Tags>();

                theTags = Tags.StringToList(nc.TagLine, edited.Id, edited.NoteFileId, edited.ArchiveId);

                if (theTags.Count > 0)
                {
                    await _db.Tags.AddRangeAsync(theTags);
                    await _db.SaveChangesAsync();
                }
            }


            _db.Entry(edited).State = EntityState.Modified;

            NoteFile nf = await NoteDataManager.GetFileById(_db, edited.NoteFileId);
            // Set file edit datetime
            nf.LastEdited = edited.LastEdited;
            _db.Entry(nf).State = EntityState.Modified;

            // set base note edit datetime
            NoteHeader bnh = await NoteDataManager.GetEditedNoteHeader(_db, edited);
            bnh.ThreadLastEdited = edited.LastEdited;
            _db.Entry(bnh).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Check for linked notefile(s)

            List<LinkedFile> links = await _db.LinkedFile.Where(p => p.HomeFileId == edited.NoteFileId && p.SendTo).ToListAsync();

            if (links == null || links.Count < 1)
            {

            }
            else
            {
                foreach (var link in links)
                {
                    if (link.SendTo)
                    {
                        LinkQueue q = new LinkQueue
                        {
                            Activity = LinkAction.Edit,
                            LinkGuid = edited.LinkGuid,
                            LinkedFileId = edited.NoteFileId,
                            BaseUri = link.RemoteBaseUri
                        };

                        _db.LinkQueue.Add(q);
                        await _db.SaveChangesAsync();

                        LinkProcessor lp = new LinkProcessor(_db);

                        BackgroundJob.Enqueue(() => lp.ProcessLinkAction(q.Id));
                    }
                }
            }

            return RedirectToAction("Display", new { id = edited.Id });
        }

        /// <summary>
        /// Goto Base Note Next or Previous
        /// </summary>
        /// <param name="id">NoteID of current Note</param>
        /// <param name="inc">1 or -1 for going forward 1 or backward 1</param>
        /// <returns></returns>
        public async Task<long?> GotoBase(long? id, int inc)
        {
            if (id == null)
            {
                return null;
            }
            NoteHeader nc = await NoteDataManager.GetNoteById(_db, (long)id);
            if (nc == null)
            {
                return null;
            }
            long? nextId = await FindBaseNoteID(nc.NoteFileId, nc.ArchiveId, nc.NoteOrdinal + inc);
            return nextId;
        }

        /// <summary>
        /// Goto Next Base Note
        /// </summary>
        /// <param name="id">NoteID of current Note</param>
        /// <returns></returns>
        public async Task<IActionResult> NextBase(long? id)
        {
            long? newid = await GotoBase(id, 1);
            if (newid == null)
            {
                // ReSharper disable once PossibleInvalidOperationException
                NoteHeader nc = await NoteDataManager.GetNoteById(_db, (long)id);
                return RedirectToAction("Listing", new { id = nc.NoteFileId });
            }
            return RedirectToAction("Display", new { id = newid });
        }

        /// <summary>
        /// Goto previous Base Note
        /// </summary>
        /// <param name="id">NoteID of current Note</param>
        /// <returns></returns>
        public async Task<IActionResult> PrevBase(long? id)
        {
            return RedirectToAction("Display", new { id = await GotoBase(id, -1) });
        }

        /// <summary>
        /// Goto Next or Previous Response or Note
        /// </summary>
        /// <param name="id">Current NoteID</param>
        /// <param name="inc">1 or -1 for forward or backward</param>
        /// <returns></returns>
        public async Task<long?> Goto(long? id, int inc)
        {
            if (id == null)
            {
                return null;
            }
            NoteHeader nc = await NoteDataManager.GetNoteById(_db, (long)id);
            if (nc == null)
            {
                return null;
            }
            var nextId = await NoteDataManager.FindResponseId(_db, nc, nc.ResponseOrdinal + inc)
                           ?? await FindBaseNoteID(nc.NoteFileId, nc.ArchiveId, nc.NoteOrdinal + inc);
            return nextId;
        }

        /// <summary>
        /// Goto Next Response or Base Note
        /// </summary>
        /// <param name="id">Current NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> NextNote(long? id)
        {
            long? newid = await Goto(id, 1);
            if (newid == null)
            {
                // ReSharper disable once PossibleInvalidOperationException
                NoteHeader nc = await NoteDataManager.GetNoteById(_db, (long)id);
                return RedirectToAction("Listing", new { id = nc.NoteFileId });
            }
            return RedirectToAction("Display", new { id = newid });
        }

        /// <summary>
        /// Goto Previous Response or Base Note
        /// </summary>
        /// <param name="id">Current NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> PrevNote(long? id)
        {
            return RedirectToAction("Display", new { id = await Goto(id, -1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TypedInputIndex(string typedInput)
        {
            ViewBag.Message = "Not a valid navigation spec: " + typedInput;

            string sfileId = Request.Form["fileID"];
            int fileId = int.Parse(sfileId);
            int noteOrd = 1;
            NoteHeader nc;
            if (string.IsNullOrEmpty(typedInput) || string.IsNullOrWhiteSpace(typedInput))
                return RedirectToAction("Listing", new { id = fileId });

            if (typedInput.Contains("."))
            {
                string[] splits = typedInput.Split(new[] { '.' });
                if (splits.Length != 2)
                {
                    return View("Error");
                }
                bool ax = !int.TryParse(splits[0], out noteOrd);
                bool bx = !int.TryParse(splits[1], out var respOrd);
                if (ax || bx)
                {
                    return View("Error");
                }
                nc = await _db.NoteHeader
                    .Where(a => a.NoteFileId == fileId && a.ArchiveId == (int)HttpContext.Session.GetInt32("ArchiveID")
                        && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == respOrd)
                    .FirstOrDefaultAsync();
            }
            else
            {
                if (!int.TryParse(typedInput, out noteOrd))
                {
                    return View("Error");
                }
                nc = await _db.NoteHeader
                    .Where(a => a.NoteFileId == fileId && a.ArchiveId == (int)HttpContext.Session.GetInt32("ArchiveID")
                        && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == 0)
                    .FirstOrDefaultAsync();
            }

            if (nc == null)
            {
                ViewBag.Message = "Note not found: " + typedInput;
                return View("Error");

            }

            return RedirectToAction("Display", new { id = nc.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TypedInputDisplay(string typedInput)
        {
            ViewBag.Message = "Not a valid navigation spec: " + typedInput;

            string sfileId = Request.Form["fileID"];
            string sOrd = Request.Form["noteord"];
            string sResp = Request.Form["respord"];
            string noteid = Request.Form["noteid"];
            int fileId = int.Parse(sfileId);
            int iOrd = int.Parse(sOrd);
            int iResp = int.Parse(sResp);
            long iNoteId = long.Parse(noteid);
            int noteOrd = 1;
            NoteHeader nc;
            bool ax = false;
            bool plus = false;
            bool minus = false;

            if (string.IsNullOrEmpty(typedInput) || string.IsNullOrWhiteSpace(typedInput))
                return RedirectToAction("NextNote", new { id = iNoteId });

            if (typedInput.StartsWith("+"))
                plus = true;
            if (typedInput.StartsWith("-"))
                minus = true;
            typedInput = typedInput.Replace("+", "").Replace("-", "");

            if (typedInput.Contains("."))
            {
                string[] splits = typedInput.Split(new[] { '.' });
                if (splits.Length != 2)
                {
                    return View("Error");
                }
                if (string.IsNullOrEmpty(splits[0]) || string.IsNullOrWhiteSpace(splits[0]))
                    noteOrd = iOrd;
                else
                    ax = !int.TryParse(splits[0], out noteOrd);
                bool bx = !int.TryParse(splits[1], out var respOrd);
                if (ax || bx)
                {
                    return View("Error");
                }

                if (noteOrd == iOrd && (plus || minus))
                {
                    if (plus)
                        respOrd += iResp;
                    else
                        respOrd = iResp - respOrd;

                    if (respOrd < 0)
                        respOrd = 0;
                    NoteHeader bnh = await NoteDataManager.GetBaseNoteHeaderForOrdinal(_db, fileId, (int)HttpContext.Session.GetInt32("ArchiveID"), noteOrd);

                    if (respOrd > bnh.ResponseCount) respOrd = bnh.ResponseCount;
                }

                nc = await _db.NoteHeader
                    .Where(a => a.NoteFileId == fileId && a.ArchiveId == (int)HttpContext.Session.GetInt32("ArchiveID") && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == respOrd)
                    .FirstOrDefaultAsync();
            }
            else
            {
                if (!int.TryParse(typedInput, out noteOrd))
                {
                    return View("Error");
                }

                if (!plus && !minus && (noteOrd == 0))
                {
                    return RedirectToAction("Listing", new { id = fileId });
                }
                if (plus)
                    noteOrd += iOrd;
                else if (minus)
                    noteOrd = iOrd - noteOrd;

                if (noteOrd < 1) noteOrd = 1;

                int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

                //NoteFile z = await NoteDataManager.GetFileByIdWithHeaders(_db, fileId, arcId);
                //List<NoteHeader> bnhl = z.NoteHeaders.Where(p => p.ResponseOrdinal == 0).ToList();

                List<NoteHeader> bnhl = await NoteDataManager.GetBaseNoteHeaders(_db, fileId, arcId);

                long cnt = bnhl.LongCount();

                if (noteOrd > cnt) noteOrd = (int)cnt;

                nc = await _db.NoteHeader
                    .Where(a => a.NoteFileId == fileId && a.ArchiveId == (int)HttpContext.Session.GetInt32("ArchiveID") && a.NoteOrdinal == noteOrd && a.ResponseOrdinal == 0)
                    .FirstOrDefaultAsync();
            }

            if (nc == null)
            {
                ViewBag.Message = "Note not found: " + typedInput;
                return View("Error");
            }

            return RedirectToAction("Display", new { id = nc.Id });
        }

        // 
        // GET: NoteDisplay/Delete/5
        // Set up to Delete a base note and all responses
        /// <summary>
        /// Deletes a base note and all responses
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            NoteHeader note = await NoteDataManager.GetNoteById(_db, (long)id);

            if (note == null)
            {
                return NotFound();
            }

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteXModel model = await GetNoteExtras(note);

            model.myAccess = await GetMyAccess(note.NoteFileId, arcId);

            if (!model.CanDelete)
            {
                ViewData["Note"] = note;
                return RedirectToAction("NoDelete", new { id });
            }
            return View(note);
        }

        /// <summary>
        /// Inform user note can not be deleted
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> NoDelete(long id)
        {
            NoteHeader note = await NoteDataManager.GetNoteById(_db, id);

            if (note == null)
            {
                return NotFound();
            }

            NoteXModel model = await GetNoteExtras(note);

            return View(model);
        }

        // POST: NoteDisplay/Delete/5
        /// <summary>
        /// Process a Note Deletion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            //_userManager.GetUserId(User);
            NoteHeader nc = await NoteDataManager.GetNoteById(_db, id);
            await NoteDataManager.DeleteNote(_db, nc);

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

            return RedirectToAction("Listing", new { id = nc.NoteFileId });
        }

        ///// <summary>
        ///// Export a file as txt, html (flat), html (expandable)
        ///// </summary>
        ///// <param name="id">NoteFileID</param>
        ///// <returns></returns>
        //public IActionResult Export(int id)
        //{
        //    NoteFile nf = db.NoteFile
        //        .Where(p => p.NoteFileID == id)
        //        .FirstOrDefault();

        //    // set up defaults
        //    NFViewModel model = new NFViewModel();
        //    model.isHtml = true;
        //    model.isCollapsible = true;
        //    model.directOutput = false;
        //    model.FileName = nf.NoteFileName;

        //    // send to view
        //    return View(model);
        //}

        /// <summary>
        /// Display NoteFile as expandable HTML
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <param name="id2">NoteOrdinal</param>
        /// <returns></returns>
        public async Task<IActionResult> AsHtml(int id, int id2, int arcId)
        {
            Notes2021.Models.ExportViewModel model = new Notes2021.Models.ExportViewModel()
            {
                directOutput = true,
                isCollapsible = true,
                isHtml = true,
                NoteOrdinal = id2
            };
            NoteFile nfl = await NoteDataManager.GetFileById(_db, id);
            model.FileName = nfl.NoteFileName;

            ExportController exp = new ExportController(_appEnv, _userManager, _signInManager,/* null, null,*/ _db);
            MemoryStream ms = await exp.DoExport(model, User, arcId);

            return new FileStreamResult(ms, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/html"));

        }

        /// <summary>
        /// Display NoteFile as "Flat" HTML
        /// </summary>
        /// <param name="id">NoteFileID</param>
        /// <param name="id2">NoteOrdinal</param>
        /// <returns></returns>
        public async Task<IActionResult> AsHtmlAlt(int id, int id2, int arcId)
        {
            ExportController exp = new ExportController(_appEnv, _userManager, _signInManager, _db);

            Notes2021.Models.ExportViewModel model = new Notes2021.Models.ExportViewModel()
            {
                directOutput = true,
                isCollapsible = false,
                isHtml = true,
                NoteOrdinal = id2
            };
            NoteFile nfl = await NoteDataManager.GetFileById(_db, id);
            model.FileName = nfl.NoteFileName;

            MemoryStream ms = await exp.DoExport(model, User, arcId);

            return new FileStreamResult(ms, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        }

        public IActionResult Export(int id)
        {
            return RedirectToAction("PreExport", "Export", new { id });
        }


        public IActionResult MailFileAsHtml(int id, int id2)
        {
            Notes2021.Models.ForwardViewModel fv = new Notes2021.Models.ForwardViewModel()
            {
                IsAdmin = User.IsInRole("Admin"),
                toAllUsers = false,

                FileID = id,
                NoteOrdinal = id2
            };
            return View(fv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MailFileAsHtml(Notes2021.Models.ForwardViewModel fv)
        {
            bool toall = fv.toAllUsers;

            return RedirectToAction("MailAsHtml", new { id = fv.FileID, id2 = fv.NoteOrdinal, id3 = fv.ToEmail, id4 = toall });
        }


        public async Task<IActionResult> MailAsHtml(int id, int id2, string id3, bool id4, int arcId)
        {
            Notes2021.Models.ExportViewModel model = new Notes2021.Models.ExportViewModel()
            {
                directOutput = true,
                isCollapsible = false,
                isHtml = true,
                NoteOrdinal = id2
            };
            NoteFile nfl = await NoteDataManager.GetFileById(_db, id);
            model.FileName = nfl.NoteFileName;

            ExportController exp = new ExportController(_appEnv, _userManager, _signInManager, _db);
            MemoryStream ms = await exp.DoExport(model, User, arcId);

            StreamReader sr = new StreamReader(ms);
            string txt = await sr.ReadToEndAsync();

            await _emailSender.SendEmailAsync(id3, "From Notes 2021 - " + nfl.NoteFileName, txt);


            //TODO low priority

            //if (id4 && User.IsInRole("Admin"))
            //{
            //    List<ApplicationUser> users = await _db.ApplicationUsers.ToListAsync();
            //    foreach (ApplicationUser user in users)
            //    {
            //        await _emailSender.SendEmailAsync(user.Email, "From Notes 2021 - " + nfl.NoteFileName, txt);
            //    }
            //}

            return RedirectToAction("Listing", new { id });
        }

        public IActionResult MailStringAsHtml(int id, int id2)
        {
            Notes2021.Models.ForwardViewModel fv = new Notes2021.Models.ForwardViewModel()
            {
                FileID = id,
                NoteOrdinal = id2,
                IsAdmin = User.IsInRole("Admin"),
                toAllUsers = false
            };
            return View(fv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MailStringAsHtml(Notes2021.Models.ForwardViewModel fv)
        {
            bool toall = fv.toAllUsers;

            return RedirectToAction("MailAsHtml", new { id = fv.FileID, id2 = fv.NoteOrdinal, id3 = fv.ToEmail, id4 = toall });
        }

        public async Task<IActionResult> Forward(long? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteHeader nc = await NoteDataManager.GetNoteById(_db, (long)id);

            Notes2021.Models.ForwardViewModel model = new Notes2021.Models.ForwardViewModel()
            {
                NoteID = nc.Id,
                NoteSubject = nc.NoteSubject,
                wholestring = false
            };
            List<NoteHeader> bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nc.NoteFileId, arcId, nc.NoteOrdinal);
            NoteHeader bnh = bnhl[0];
            model.hasstring = bnh.ResponseCount > 0;

            NoteAccess myacc = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), nc.NoteFileId, arcId);
            if (myacc == null || !myacc.ReadAccess)
            {
                ViewBag.Message = "You cannot read file " + nc.NoteFile.NoteFileName;
                return View("Error");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forward(Notes2021.Models.ForwardViewModel fv)
        {
            if (fv == null)
            {
                return BadRequest();
            }

            //NoteHeader nc = await NoteDataManager.GetNoteByIdWithFile(_db, fv.NoteID);

            UserData user = await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User));
            string userName = user.DisplayName;

            string userEmail = User.Identity.Name;


            //TODO!! put this in app!!
            //await NoteDataManager.SendNotesAsync(fv, _db, _emailSender, userEmail, userName, Globals.ProductionUrl);

            return RedirectToAction("Display", new { id = fv.NoteID });
        }

        //  Start of Sequencer code

        /// <summary>
        /// Begin running the sequencer
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> BeginSequence()     // begin first file in list with first unread note
        {
            string userid = _userManager.GetUserId(User);
            List<Sequencer> seqList = await NoteDataManager.GetSeqListForUser(_db, userid);

            if (seqList.Count == 0)
            {
                return RedirectToAction("Index", "Sequencers");
            }

            Sequencer myseqfile = await FirstSeq();

            if (myseqfile == null)
                return RedirectToAction("CompleteSequence");

            List<NoteHeader> sbnh = await NoteDataManager.GetSbnh(_db, myseqfile);

            while (!sbnh.Any())
            {
                myseqfile = await NextSeq();
                if (myseqfile == null)
                    return RedirectToAction("CompleteSequence");

                sbnh = await NoteDataManager.GetSbnh(_db, myseqfile);
            }

            NoteHeader item = sbnh.First();
            return RedirectToAction("Display", new { id = item.Id });
        }



        /// <summary>
        /// Continue sequencing
        /// </summary>
        /// <param name="id">NoteID to start from</param>
        /// <returns></returns>
        public async Task<IActionResult> ContinueSequence(long id)      // next unread note in file
        {
            string userid = _userManager.GetUserId(User);
            List<Sequencer> list = await _db.Sequencer
                .Where(x => x.UserId == userid && x.Active)
                .OrderBy(x => x.Ordinal)
                .ToListAsync();

            Sequencer myseqfile = list.FirstOrDefault();

            var item0 = await NoteDataManager.GetNoteById(_db, id);

            NoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, item0.NoteFileId, 0, item0.NoteOrdinal);

            // check for responses beyond current base/resp

            var myseqfile1 = myseqfile;
            var bnh1 = bnh;
            var snc = from x in _db.NoteHeader
                      where x.NoteFileId == myseqfile1.NoteFileId && x.NoteOrdinal == bnh1.NoteOrdinal && x.ResponseOrdinal > item0.ResponseOrdinal && x.LastEdited >= myseqfile1.LastTime
                      orderby x.ResponseOrdinal
                      select new
                      {
                          noteid = x.Id   // get only NoteID
                      };

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (bnh != null && snc != null && snc.Any())
            {
                long current = snc.First().noteid;
                return RedirectToAction("Display", new { id = current });
            }

            // else we go to next base note that qualifies

            List<NoteHeader> sbnh = await NoteDataManager.GetSeqHeader1(_db, myseqfile, bnh);

            while (!sbnh.Any())  // next file
            {
                myseqfile = await NextSeq();
                if (myseqfile == null)
                    return RedirectToAction("CompleteSequence");
                sbnh = await NoteDataManager.GetSeqHeader2(_db, myseqfile);

            }
            // next base note
            bnh = sbnh.First();
            return RedirectToAction("Display", new { id = bnh.Id });
        }

        // Inform user sequencing is done
        public IActionResult CompleteSequence()
        {
            HttpContext.Session.Remove("CurrentSeq");
            return View();
        }

        /// <summary>
        /// Get first row from user's sequencer list
        /// </summary>
        /// <returns></returns>
        public async Task<Sequencer> FirstSeq()   // get first sequencer data line for user
        {
            HttpContext.Session.Remove("CurrentSeq");

            string userid = _userManager.GetUserId(User);
            List<Sequencer> list = await NoteDataManager.GetSeqListForUser(_db, userid);

            Sequencer item = list.First();
            item.StartTime = DateTime.Now.ToUniversalTime();
            item.Active = true;
            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32("CurrentSeq", 1);
            return item;
        }

        /// <summary>
        /// Find next note that qualifies
        /// </summary>
        /// <returns></returns>
        public async Task<Sequencer> NextSeq()
        {
            bool found = false;
            HttpContext.Session.Remove("CurrentSeq");

            string userid = _userManager.GetUserId(User);
            List<Sequencer> mylist = await NoteDataManager.GetSeqListForUser(_db, userid);

            foreach (Sequencer item in mylist)
            {
                if (item.Active && !found) // end this file
                {
                    item.Active = false;
                    item.LastTime = item.StartTime;
                    _db.Entry(item).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    found = true;
                    HttpContext.Session.Remove("CurrentSeq");
                }
                else if (found) // start next file
                {
                    item.StartTime = DateTime.Now.ToUniversalTime();
                    item.Active = true;
                    _db.Entry(item).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    HttpContext.Session.SetInt32("CurrentSeq", 1);
                    return item;
                }
            }
            return null;
        }

        // Searching code begins here

        /// <summary>
        /// Start for a search that begins from Index
        /// </summary>
        /// <param name="id">FileID</param>
        /// <returns></returns>
        public async Task<IActionResult> SearchFromIndex(int id)
        {
            long? firstNoteId = await FindBaseNoteID(id, (int)HttpContext.Session.GetInt32("ArchiveID"), 1);
            return RedirectToAction("Search", new { id = firstNoteId });
        }

        /// <summary>
        /// Start a search from a note
        /// </summary>
        /// <param name="id">NoteID</param>
        /// <returns></returns>
        public async Task<IActionResult> Search(long id)
        {
            NoteHeader nc = await NoteDataManager.GetNoteById(_db, id);

            TZone tzone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            //Setup a Search Object with defaults
            Search search = new Search()
            {
                BaseOrdinal = nc.NoteOrdinal,
                NoteFileId = nc.NoteFileId,
                NoteID = nc.Id,
                Option = Notes2021Blazor.Shared.SearchOption.Content,
                ResponseOrdinal = nc.ResponseOrdinal,
                Time = tzone.Local(DateTime.Now.ToUniversalTime()),
                UserId = _userManager.GetUserId(User)
            };
            List<SelectListItem> list2 = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "-1",
                    Text = "-- Select a Search Option --"
                },
                new SelectListItem
                {
                    Value = "0",
                    Text = "Author"
                },
                new SelectListItem
                {
                    Value = "1",
                    Text = "Title"
                },
                new SelectListItem
                {
                    Value = "2",
                    Text = "Content"
                },
                new SelectListItem
                {
                    Value = "3",
                    Text = "Tag"
                },
                new SelectListItem
                {
                    Value = "4",
                    Text = "Director Message"
                },
                new SelectListItem
                {
                    Value = "5",
                    Text = "Time After"
                },
                new SelectListItem
                {
                    Value = "6",
                    Text = "Time Before"
                }
            };
            ViewBag.OptionList = list2;

            // Send Search Object to View to Gather user input for search
            return View(search);
        }

        /// <summary>
        /// Execute search
        /// </summary>
        /// <param name="search">Search Object</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(Search search)
        {
            search.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                TZone tzone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

                if (!string.IsNullOrEmpty(search.Text))
                {
                    search.Text = search.Text.ToLower();
                    if (search.Option == Notes2021Blazor.Shared.SearchOption.Tag)
                        search.Text = search.Text.Trim();
                }

                HttpContext.Session.SetInt32("IsTagSearch", 0);

                if (search.Option == Notes2021Blazor.Shared.SearchOption.Tag)
                {
                    List<long> taggedIds = await MakeTagSearchResultsList(search);
                    if (taggedIds != null && taggedIds.Count > 0)
                    {
                        // ReSharper disable once InvokeAsExtensionMethod
                        SessionExtensionsLocal.SetObject(HttpContext.Session, "TaggedIds", taggedIds);

                        HttpContext.Session.SetInt32("IsSearch", 1);
                        HttpContext.Session.SetInt32("IsTagSearch", 1);

                        // View Note found in Search
                        return RedirectToAction("Display", new { id = taggedIds[0] });

                    }
                    HttpContext.Session.SetInt32("IsSearch", 0);
                    return RedirectToAction("Listing", new { id = search.NoteFileId });   // nothing new found   -- temp behavior??
                }


                // Perform a search
                Search newSpecs = await DoSearch(search, tzone);

                // Get saved previous Specs
                string userid = _userManager.GetUserId(User);
                Search mysearch = await NoteDataManager.GetUserSearch(_db, userid);

                if (mysearch != null)  // Remove old specs
                {
                    _db.Search.Remove(mysearch);
                    await _db.SaveChangesAsync();
                }

                if (newSpecs.NoteID == search.NoteID || newSpecs.NoteID == 0)
                {
                    HttpContext.Session.SetInt32("IsSearch", 0);
                    return RedirectToAction("Display", new { id = search.NoteID });   // nothing new found   -- temp behavior??
                }

                await _db.Database.OpenConnectionAsync();
                try
                {
                    // Save new specs
                    await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Search ON");
                    await _db.Search.AddAsync(newSpecs);
                    await _db.SaveChangesAsync();
                    await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Search OFF");
                }
                finally
                {
                    _db.Database.CloseConnection();
                }

                HttpContext.Session.SetInt32("IsSearch", 1);

                // View Note found in Search
                return RedirectToAction("Display", new { id = newSpecs.NoteID });
            }
            return View(search);
        }

        /// <summary>
        /// Continue a search
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ContinueSearch()
        {
            if (HttpContext.Session.GetInt32("IsTagSearch") == 1)
            {
                // ReSharper disable once InvokeAsExtensionMethod
                List<long> taggedIds = SessionExtensionsLocal.GetObject<List<long>>(HttpContext.Session, "TaggedIds");
                NoteHeader nh = await NoteDataManager.GetNoteHeader(_db, taggedIds[0]);
                if (taggedIds != null && taggedIds.Count > 0)
                {
                    if (taggedIds.Count < 2)
                    {
                        HttpContext.Session.Remove("TaggedIds");
                        HttpContext.Session.SetInt32("IsSearch", 0);
                        HttpContext.Session.SetInt32("IsTagSearch", 0);

                        return RedirectToAction("Listing", new { id = nh.NoteFileId });   // nothing new found   -- temp behavior??
                    }

                    taggedIds.RemoveAt(0);
                    SessionExtensionsLocal.SetObject(HttpContext.Session, "TaggedIds", taggedIds);

                    HttpContext.Session.SetInt32("IsSearch", 1);
                    HttpContext.Session.SetInt32("IsTagSearch", 1);

                    // View Note found in Search
                    return RedirectToAction("Display", new { id = taggedIds[0] });

                }
                HttpContext.Session.SetInt32("IsSearch", 0);
                HttpContext.Session.SetInt32("IsTagSearch", 0);

                return RedirectToAction("Listing", new { id = nh.NoteFileId });   // nothing new found   -- temp behavior??
            }

            // Get saved search specs
            string userid = _userManager.GetUserId(User);
            Search search = await NoteDataManager.GetUserSearch(_db, userid);

            if (search != null)  // remove specs
            {
                _db.Search.Remove(search);
                await _db.SaveChangesAsync();
            }

            TZone tzone = await LocalManager.GetUserTimeZone(HttpContext, User, _userManager, _signInManager, _db);

            // perform continued search
            Search newSpecs = await DoSearch(search, tzone);
            if (search != null && (newSpecs.NoteID == search.NoteID || newSpecs.NoteID == 0))
            {
                HttpContext.Session.SetInt32("IsSearch", 0);
                return RedirectToAction("Display", new { id = search.NoteID });   // nothing new found   -- temp behavior
            }

            await _db.Database.OpenConnectionAsync();
            try
            {
                // Save new specs
                await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Search ON");
                await _db.Search.AddAsync(newSpecs);
                await _db.SaveChangesAsync();
                await _db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Search OFF");
            }
            finally
            {
                _db.Database.CloseConnection();
            }


            HttpContext.Session.SetInt32("IsSearch", 1);

            // display note matching search
            return RedirectToAction("Display", new { id = newSpecs.NoteID });
        }

        /// <summary>
        /// Test a note for a match to search specs
        /// </summary>
        /// <param name="nc">NoteContent</param>
        /// <param name="sv">Search</param>
        /// <param name="tzone">TimeZone</param>
        /// <returns>Search for found note</returns>
        private Search SearchTest(NoteHeader nc, Search sv, TZone tzone)
        {
            // we can search in a number of ways.  check the ONE the user selected
            switch (sv.Option)
            {
                case Notes2021Blazor.Shared.SearchOption.Author:

                    if (nc.AuthorName.ToLower().Contains(sv.Text))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.Id;
                        return x;
                    }
                    return sv;

                case Notes2021Blazor.Shared.SearchOption.Content:
                    if (nc.NoteContent.NoteBody.ToLower().Contains(sv.Text))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.Id;
                        return x;
                    }
                    return sv;

                case Notes2021Blazor.Shared.SearchOption.Tag:
                    if (nc.Tags != null && nc.Tags.Count > 0)
                    {
                        foreach (Tags item in nc.Tags)
                        {
                            if (String.CompareOrdinal(item.Tag.ToLower(), sv.Text.Trim()) == 0)
                            {
                                Search x = sv.Clone(sv);
                                x.NoteID = nc.Id;
                                return x;
                            }
                        }
                    }
                    return sv;

                case Notes2021Blazor.Shared.SearchOption.DirMess:
                    if (nc.NoteContent.DirectorMessage.ToLower().Contains(sv.Text))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.Id;
                        return x;
                    }
                    return sv;

                    case Notes2021Blazor.Shared.SearchOption.Title:
                    if (nc.NoteSubject.ToLower().Contains(sv.Text))
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.Id;
                        return x;
                    }
                    return sv;

                case Notes2021Blazor.Shared.SearchOption.TimeIsAfter:
                    if (tzone.Local(nc.LastEdited) >= sv.Time)
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.Id;
                        return x;
                    }
                    return sv;

                case Notes2021Blazor.Shared.SearchOption.TimeIsBefore:
                    if (tzone.Local(nc.LastEdited) <= sv.Time)
                    {
                        Search x = sv.Clone(sv);
                        x.NoteID = nc.Id;
                        return x;
                    }
                    return sv;

            }
            return sv;
        }

        /// <summary>
        /// Performs a search give a Search that contains specs for search
        /// where we are in the search
        /// </summary>
        /// <param name="start">Search</param>
        /// <param name="tzone">TimeZone</param>
        /// <returns>Search for search match</returns>
        private async Task<Search> DoSearch(Search start, TZone tzone)
        {
            var item0 = await NoteDataManager.GetNoteById(_db, start.NoteID);
            int myRespOrdinal = item0.ResponseOrdinal;
            NoteHeader bnh = await GetBaseNoteHeader(item0);

        // check for responses beyond current base/resp

        Repeat:
            List<NoteHeader> snc = await NoteDataManager.GetSearchResponseList(_db, start, myRespOrdinal, bnh, start.Option);

            if (snc.Any())
            {
                foreach (NoteHeader item in snc)
                {
                    Search sv = SearchTest(item, start, tzone);
                    if (sv.NoteID != start.NoteID)
                        return sv;
                }
            }

            // if none we go to next base

            List<NoteHeader> sbnh = await NoteDataManager.GetSearchHeaders(_db, start, bnh, start.Option);

            if (sbnh.Any())  // still have base notes to search
            {
                bnh = sbnh.First();
                start.ResponseOrdinal = 0;
                start.BaseOrdinal = bnh.NoteOrdinal;
                //start.NoteID = bnh.NoteID;
                myRespOrdinal = -1;
                goto Repeat;
            }

            return start;
        }

        private async Task<List<long>> MakeTagSearchResultsList(Search sSpecs)
        {
            List<long> noteIds = await _db.Tags
                .Where(x => x.NoteFileId == sSpecs.NoteFileId && x.ArchiveId == sSpecs.ArchiveId && String.CompareOrdinal(x.Tag, sSpecs.Text) == 0)
                .Select(x => x.NoteHeaderId)
                .OrderBy(x => x)
                .ToListAsync();

            return await _db.NoteHeader
                .Where(x => noteIds.Contains(x.Id))
                .OrderBy(x => x.NoteOrdinal)
                .ThenBy(x => x.ResponseOrdinal)
                .Select(x => x.Id)
                .ToListAsync();
        }



        ////   end of search


#pragma warning disable 1998
        public async Task<IActionResult> Mark(long id)
#pragma warning restore 1998
        {
            List<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem() { Value = "-2", Text = "-- Select an Option --" },
                new SelectListItem() { Value = "-1", Text = "Entire Note String" },
                new SelectListItem() { Value = "0", Text = "Base Note" },
                new SelectListItem() { Value = "1", Text = "Response" }
            };
            MarkViewModel v = new MarkViewModel()
            {
                SelectedValue = items.First().Value,
                option = items,
                NoteID = id
            };
            return View(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkProc(MarkViewModel markview)
        {
            // Get NoteContent
            NoteHeader nc = await NoteDataManager.GetNoteById(_db, markview.NoteID);

            NoteHeader bnh = await NoteDataManager.GetBaseNoteHeader(_db, nc.NoteFileId, (int)HttpContext.Session.GetInt32("ArchiveID"), nc.NoteOrdinal);

            Mark markx = new Mark()
            {
                NoteHeaderId = bnh.BaseNoteId,
                NoteFileId = nc.NoteFileId,
                NoteOrdinal = nc.NoteOrdinal,
                UserId = _userManager.GetUserId(User)
            };
            var markx1 = markx;
            var q = _db.Mark
                .Where(p => p.UserId == markx1.UserId);

            int ord = q.Count() + 1;

            markx.MarkOrdinal = ord;

            if (markview.SelectedValue == "-1")
            {
                markx.ResponseOrdinal = 0;
                _db.Mark.Add(markx);

                for (int i = 1; i <= bnh.ResponseCount; i++)
                {
                    markx = new Mark()
                    {
                        NoteHeaderId = bnh.BaseNoteId,
                        NoteFileId = nc.NoteFileId,
                        NoteOrdinal = nc.NoteOrdinal,
                        UserId = _userManager.GetUserId(User),
                        MarkOrdinal = ++ord,
                        ResponseOrdinal = i
                    };
                    _db.Mark.Add(markx);
                }
            }
            else if (markview.SelectedValue == "0")
            {
                markx.ResponseOrdinal = 0;
                _db.Mark.Add(markx);
            }
            else if (markview.SelectedValue == "1")
            {
                markx.ResponseOrdinal = nc.ResponseOrdinal;
                _db.Mark.Add(markx);
            }

            await _db.SaveChangesAsync();

            return View("Marked");
        }


        public async Task<IActionResult> MarkMine(int id)
        {
            string userid = _userManager.GetUserId(User);
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            List<NoteHeader> notesHeaders = _db.NoteHeader
                .Where(p => p.AuthorID == userid && p.NoteFileId == id && p.ArchiveId == arcId)
                .OrderBy(p => p.NoteOrdinal)
                .ThenBy(p => p.ResponseOrdinal)
                .ToList();

            int ord = 0;

            if (notesHeaders != null && notesHeaders.Count > 0)
            {
                Mark markx = new Mark();
                foreach (var itemHeader in notesHeaders)
                {
                    markx = new Mark()
                    {
                        NoteHeaderId = itemHeader.Id,
                        NoteFileId = id,
                        NoteOrdinal = itemHeader.NoteOrdinal,
                        UserId = userid,
                        ResponseOrdinal = itemHeader.ResponseOrdinal,
                        MarkOrdinal = ++ord
                    };
                    await _db.Mark.AddAsync(markx);
                }
                await _db.SaveChangesAsync();

                return RedirectToAction("ExportMarked", "Export", new { id });
            }
            return RedirectToAction("Viewit", "NoteFileList", new { id });
        }

        public async Task<IActionResult> Copy(long? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteHeader nc = await NoteDataManager.GetNoteById(_db, (long)id);

            CopyViewModel model = new CopyViewModel()
            {
                NoteID = nc.Id,
                NoteSubject = nc.NoteSubject,
                Wholestring = false
            };
            List<NoteHeader> bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nc.NoteFileId, arcId, nc.NoteOrdinal);
            NoteHeader bnh = bnhl[0];
            model.Hasstring = bnh.ResponseCount > 0;

            NoteAccess myacc = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), nc.NoteFileId, arcId);
            if (myacc == null || !myacc.ReadAccess)
            {
                ViewBag.Message = "You cannot read file " + nc.NoteFile.NoteFileName;
                return View("Error");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Copy(CopyViewModel cv)
        {
            if (cv == null)
            {
                return BadRequest();
            }
            int arcId = (int)HttpContext.Session.GetInt32("ArchiveID");

            NoteFile nf2 = await NoteDataManager.GetFileByName(_db, cv.ToFile);
            if (nf2 == null)
            {
                ViewBag.Message = "Copy file does not exist: " + cv.ToFile;
                return View("Error");
            }

            NoteAccess myacc = await AccessManager.GetAccess(_db, _userManager.GetUserId(User), nf2.Id, arcId);
            if (myacc == null || !myacc.Write)
            {
                ViewBag.Message = "You cannot write file " + nf2.NoteFileName;
                return View("Error");
            }

            NoteHeader nc = await NoteDataManager.GetNoteByIdWithFile(_db, cv.NoteID);
            //ApplicationUser user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            if (!cv.Hasstring || !cv.Wholestring)
            {
                NoteContent newContent = new NoteContent()
                {
                    NoteBody = "<p><strong>** =" + nc.NoteFile.NoteFileName + " - " + nc.AuthorName
                               + " - " + nc.LastEdited.ToShortDateString() + " "
                               + nc.LastEdited.ToShortTimeString() + " UCT" + " **</strong></p>"
                };
                if (!string.IsNullOrEmpty(nc.NoteContent.DirectorMessage))
                {
                    newContent.DirectorMessage = nc.NoteContent.DirectorMessage;
                }

                NoteHeader ncc = new NoteHeader()
                {
                    LastEdited = DateTime.Now.ToUniversalTime(),
                    ThreadLastEdited = DateTime.Now.ToUniversalTime(),
                    CreateDate = DateTime.Now.ToUniversalTime()
                };
                newContent.NoteBody += nc.NoteContent.NoteBody;
                ncc.NoteFileId = nf2.Id;

                //do use proper name
                //ncc.AuthorName = (await _userManager.GetUserAsync(User)).DisplayName;
                ncc.AuthorName = (await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User)))
                    .DisplayName;

                ncc.AuthorID = _userManager.GetUserId(User);

                ncc.NoteSubject = nc.NoteSubject;
                ncc.ResponseOrdinal = 0;

                string tags = string.Empty;
                if (nc.Tags != null && nc.Tags.Count > 0)
                {
                    foreach (Tags item in nc.Tags)
                    {
                        tags += item.Tag + " ";
                    }
                }

                await NoteDataManager.CreateNote(_db, _userManager, ncc, newContent.NoteBody, tags, newContent.DirectorMessage, true, false);
            }
            else
            {
                List<NoteHeader> bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nc.NoteFileId, arcId, nc.NoteOrdinal);
                NoteHeader bnh = bnhl[0];
                cv.NoteSubject = bnh.NoteSubject;
                List<NoteHeader> notes = await NoteDataManager.GetBaseNoteAndResponses(_db, nc.NoteFileId, arcId, nc.NoteOrdinal);

                NoteHeader nbnh = null;

                for (int i = 0; i < notes.Count; i++)
                {
                    if (i == 0)
                    {
                        NoteContent newContent = new NoteContent()
                        {
                            NoteBody = "<p><strong>** =" + notes[0].NoteFile.NoteFileName + " - " + notes[0].AuthorName + " - "
                            + notes[0].LastEdited.ToShortDateString() + " " + notes[0].LastEdited.ToShortTimeString() + " UCT" + " **</strong></p>"
                        };
                        if (!string.IsNullOrEmpty(notes[0].NoteContent.DirectorMessage))
                        {
                            newContent.DirectorMessage = notes[0].NoteContent.DirectorMessage;
                        }
                        NoteHeader ncc = new NoteHeader()
                        {
                            LastEdited = DateTime.Now.ToUniversalTime(),
                            ThreadLastEdited = DateTime.Now.ToUniversalTime(),
                            CreateDate = DateTime.Now.ToUniversalTime()
                        };
                        newContent.NoteBody += notes[0].NoteContent.NoteBody;

                        ncc.NoteFileId = nf2.Id;
                        ncc.AuthorName = (await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User)))
                            .DisplayName;
                        ncc.AuthorID = _userManager.GetUserId(User);

                        ncc.NoteSubject = notes[0].NoteSubject;
                        ncc.ResponseOrdinal = 0;

                        string tags = string.Empty;
                        //if (notes[0].Tags != null && notes[0].Tags.Count > 0)
                        //{
                        //    foreach (Tags item in notes[0].Tags)
                        //    {
                        //        tags += item.Tag + " ";
                        //    }
                        //}

                        tags = Tags.ListToString(notes[0].Tags);

                        var baseNote = await NoteDataManager.CreateNote(_db, _userManager, ncc, newContent.NoteBody, tags, newContent.DirectorMessage, true, false);
                        var nbnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, baseNote.NoteFileId, baseNote.ArchiveId, baseNote.NoteOrdinal);
                        nbnh = nbnhl[0];
                    }
                    else
                    {
                        NoteContent newContent = new NoteContent()
                        {
                            NoteBody = "<p><strong>** =" + notes[i].NoteFile.NoteFileName + " - " + notes[i].AuthorName + " - "
                            + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UCT" + " **</strong></p>"
                        };
                        if (!string.IsNullOrEmpty(notes[i].NoteContent.DirectorMessage))
                        {
                            newContent.DirectorMessage = notes[i].NoteContent.DirectorMessage;
                        }
                        NoteHeader ncc = new NoteHeader()
                        {
                            LastEdited = DateTime.Now.ToUniversalTime(),
                            ThreadLastEdited = DateTime.Now.ToUniversalTime(),
                            CreateDate = DateTime.Now.ToUniversalTime()
                        };
                        newContent.NoteBody += notes[i].NoteContent.NoteBody;

                        ncc.NoteFileId = nf2.Id;
                        ncc.AuthorName = (await _db.UserData.SingleAsync(p => p.UserId == _userManager.GetUserId(User)))
                            .DisplayName;
                        ncc.AuthorID = _userManager.GetUserId(User);

                        ncc.NoteSubject = notes[i].NoteSubject;
                        ncc.ResponseOrdinal = notes[i].ResponseOrdinal;
                        // ReSharper disable once PossibleNullReferenceException
                        ncc.NoteOrdinal = nbnh.NoteOrdinal;
                        ncc.BaseNoteId = nbnh.Id;  //Fix

                        string tags = string.Empty;
                        //if (notes[i].Tags != null && notes[i].Tags.Count > 0)
                        //{
                        //    foreach (Tags item in notes[i].Tags)
                        //    {
                        //        tags += item.Tag + " ";
                        //    }
                        //}

                        tags = Tags.ListToString(notes[i].Tags);

                        await NoteDataManager.CreateResponse(_db, _userManager, ncc, newContent.NoteBody, tags, newContent.DirectorMessage, true, false);
                    }
                }
            }

            return RedirectToAction("Display", new { id = cv.NoteID });
        }


    }

    public static class SessionExtensionsLocal
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

}