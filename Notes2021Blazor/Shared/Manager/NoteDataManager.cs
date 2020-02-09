using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Notes2021Blazor.Shared
{
    public static class NoteDataManager
    {
        /// <summary>
        /// Create a NoteFile
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="userManager">UserManager</param>
        /// <param name="userId">UserID of creator</param>
        /// <param name="name">NoteFile name</param>
        /// <param name="title">NoteFile title</param>
        /// <returns></returns>
        public static async Task<bool> CreateNoteFile(NotesDbContext db,
            UserManager<IdentityUser> userManager,
            string userId, string name, string title)
        {
            var query = db.NoteFile.Where(p => p.NoteFileName == name);
            if (!query.Any())
            {
                NoteFile noteFile = new NoteFile()
                {
                    NoteFileName = name,
                    NoteFileTitle = title,
                    Id = 0,
                    OwnerId = userId,
                    LastEdited = DateTime.Now.ToUniversalTime()
                };
                db.NoteFile.Add(noteFile);
                await db.SaveChangesAsync();

                NoteFile nf = await db.NoteFile
                    .Where(p => p.NoteFileName == noteFile.NoteFileName)
                    .FirstOrDefaultAsync();

                await AccessManager.CreateBaseEntries(db, userManager, userId, nf.Id);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete a NoteFile
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="id">NoteFileID</param>
        /// <returns></returns>
        public static async Task<bool> DeleteNoteFile(NotesDbContext db, int id)
        {
            // Things to delete:
            // 1)  X Entries in NoteContent
            // 2)  X Entries in BaseNoteHeader
            // 3)  X Entries in Sequencer
            // 4)  X Entries in NoteAccesses
            // 5)  X Entries in Marks
            // 6)  X Entries in SearchView
            // 7)  1 Entry in NoteFile

            // The above (1 - 6) now done by Cascade Delete of NoteFile

            //List<NoteContent> nc = await _db.NoteContent
            //    .Where(p => p.NoteFileID == id)
            //    .ToListAsync();
            //List<BaseNoteHeader> bnh = await GetBaseNoteHeadersForFile(_db, id);
            //List<Sequencer> seq = await _db.Sequencer
            //.Where(p => p.NoteFileID == id)
            //.ToListAsync();
            //List<NoteAccess> na = await AccessManager.GetAccessListForFile(_db, id);
            //List<Mark> marks = await _db.Mark
            //    .Where(p => p.NoteFileID == id)
            //    .ToListAsync();
            //List<SearchView> sv = await _db.SearchView
            //    .Where(p => p.NoteFileID == id)
            //    .ToListAsync();

            //_db.NoteContent.RemoveRange(nc);
            //_db.BaseNoteHeader.RemoveRange(bnh);
            //_db.Sequencer.RemoveRange(seq);
            //_db.NoteAccess.RemoveRange(na);
            //_db.Mark.RemoveRange(marks);
            //_db.SearchView.RemoveRange(sv);

            NoteFile noteFile = await db.NoteFile
               .Where(p => p.Id == id)
               .FirstAsync();

            for (int arcId = 0; arcId <= noteFile.NumberArchives; arcId++)
            {
                List<NoteAccess> na = await AccessManager.GetAccessListForFile(db, id, arcId);
                db.NoteAccess.RemoveRange(na);
            }

            List<Subscription> subs = await db.Subscription
                .Where(p => p.NoteFileId == id)
                .ToListAsync();
            db.Subscription.RemoveRange(subs);

            db.NoteFile.Remove(noteFile);

            await db.SaveChangesAsync();

            return true;
        }

        public static void ArchiveNoteFile(NotesDbContext _db, NoteFile noteFile)
        {
            noteFile.NumberArchives++;
            _db.Update(noteFile);

            List<NoteHeader> nhl = _db.NoteHeader.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToList();

            foreach (NoteHeader nh in nhl)
            {
                nh.ArchiveId = noteFile.NumberArchives;
                _db.Update(nh);
            }

            List<NoteAccess> nal = _db.NoteAccess.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToList();
            foreach (NoteAccess na in nal)
            {
                na.ArchiveId = noteFile.NumberArchives;
            }
            _db.NoteAccess.AddRange(nal);

            List<Tags> ntl = _db.Tags.Where(p => p.NoteFileId == noteFile.Id && p.ArchiveId == 0).ToList();
            foreach (Tags nt in ntl)
            {
                nt.ArchiveId = noteFile.NumberArchives;
                _db.Update(nt);
            }

            _db.SaveChanges();
        }

        public static async Task<NoteHeader> CreateNote(NotesDbContext db, UserManager<IdentityUser> userManager, NoteHeader nh, string body, string tags, string dMessage, bool send, bool linked)
        {
            if (nh.ResponseOrdinal == 0)  // base note
            {
                nh.NoteOrdinal = await NextBaseNoteOrdinal(db, nh.NoteFileId, nh.ArchiveId);
            }

            if (!linked)
            {
                nh.LinkGuid = Guid.NewGuid().ToString();
            }

            if (!send) // indicates an import operation / adjust time to UCT / assume original was CST = UCT-06, so add 6 hours
            {
                int offset = 6;
                if (nh.LastEdited.IsDaylightSavingTime())
                    offset--;

                Random rand = new Random();
                int ms = rand.Next(999);

                nh.LastEdited = nh.LastEdited.AddHours(offset).AddMilliseconds(ms);
                nh.CreateDate = nh.LastEdited;
                nh.ThreadLastEdited = nh.CreateDate;
            }

            NoteFile nf = await db.NoteFile
                .Where(p => p.Id == nh.NoteFileId)
                .FirstOrDefaultAsync();

            nf.LastEdited = nh.CreateDate;
            db.Entry(nf).State = EntityState.Modified;
            db.NoteHeader.Add(nh);
            await db.SaveChangesAsync();

            NoteHeader newHeader = nh;

            if (newHeader.ResponseOrdinal == 0)
            {
                newHeader.BaseNoteId = newHeader.Id;
                db.Entry(newHeader).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            else
            {
                NoteHeader baseNote = await db.NoteHeader
                    .Where(p => p.NoteFileId == newHeader.NoteFileId && p.ArchiveId == newHeader.ArchiveId && p.NoteOrdinal == newHeader.NoteOrdinal && p.ResponseOrdinal == 0)
                    .FirstOrDefaultAsync();

                newHeader.BaseNoteId = baseNote.Id;
                db.Entry(newHeader).State = EntityState.Modified;
                await db.SaveChangesAsync();

            }

            NoteContent newContent = new NoteContent()
            {
                NoteHeaderId = newHeader.Id,
                NoteBody = body,
                DirectorMessage = dMessage
            };
            db.NoteContent.Add(newContent);
            await db.SaveChangesAsync();

            // deal with tags

            if (tags != null && tags.Length > 1)
            {
                var theTags = Tags.StringToList(tags, newHeader.Id, newHeader.NoteFileId, newHeader.ArchiveId);

                if (theTags.Count > 0)
                {
                    await db.Tags.AddRangeAsync(theTags);
                    await db.SaveChangesAsync();
                }
            }

            // Check for linked notefile(s)

            List<LinkedFile> links = await db.LinkedFile.Where(p => p.HomeFileId == newHeader.NoteFileId && p.SendTo).ToListAsync();

            if (linked || links == null || links.Count < 1)
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
                            Activity = newHeader.ResponseOrdinal == 0 ? LinkAction.CreateBase : LinkAction.CreateResponse,
                            LinkGuid = newHeader.LinkGuid,
                            LinkedFileId = newHeader.NoteFileId,
                            BaseUri = link.RemoteBaseUri,
                            Secret = link.Secret
                        };

                        db.LinkQueue.Add(q);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return newHeader;
        }



        public static async Task<NoteHeader> CreateResponse(NotesDbContext db, UserManager<IdentityUser> userManager, NoteHeader nh, string body, string tags, string dMessage, bool send, bool linked)
        {
            NoteHeader mine = await GetBaseNoteHeader(db, nh.BaseNoteId);
            db.Entry(mine).State = EntityState.Unchanged;
            await db.SaveChangesAsync();

            mine.ThreadLastEdited = DateTime.Now.ToUniversalTime();
            mine.ResponseCount++;

            db.Entry(mine).State = EntityState.Modified;
            await db.SaveChangesAsync();

            nh.ResponseOrdinal = mine.ResponseCount;
            nh.NoteOrdinal = mine.NoteOrdinal;
            return await CreateNote(db, userManager, nh, body, tags, dMessage, send, linked);
        }

        /// <summary>
        /// Delete a Note
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        public static async Task<bool> DeleteNote(NotesDbContext db, NoteHeader nc)
        {
            if (nc.ResponseOrdinal == 0)     // base note
            {
                return await DeleteBaseNote(db, nc);
            }
            else  // Response
            {
                return await DeleteResponse(db, nc);
            }
        }

        /// <summary>
        /// Delete a Base Note
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        // Steps involved:
        // 1. Delete all NoteContent rows where NoteFileID, NoteOrdinal match input
        // 2. Delete single row in BaseNoteHeader where NoteFileID, NoteOrdinal match input
        // 3. Decrement all BaseNoteHeader.NoteOrdinal where NoteFileID match input and
        //    BaseNoteHeader.NoteOrdinal > nc.NoteOrdinal
        // 4. Decrement all NoteContent.NoteOrdinal where NoteFileID match input and NoteContent.NoteOrdinal > nc.NoteOrdinal
        private static async Task<bool> DeleteBaseNote(NotesDbContext db, NoteHeader nc)
        {
            int fileId = nc.NoteFileId;
            int arcId = nc.ArchiveId;
            int noteOrd = nc.NoteOrdinal;

            try
            {
                List<NoteHeader> deleteCont = await GetNoteContentList(db, fileId, arcId, noteOrd);

                foreach (var nh in deleteCont)
                {
                    await DeleteLinked(db, nh);
                }

                db.NoteHeader.RemoveRange(deleteCont);

                List<NoteHeader> upBase = await db.NoteHeader
                    .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal > noteOrd)
                    .ToListAsync();

                foreach (var cont in upBase)
                {
                    cont.NoteOrdinal--;
                    db.Entry(cont).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        /// Delete a Response Note
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        // Steps involved:
        // 1. Delete single NoteContent row where NoteFileID, NoteOrdinal, and ResponseOrdinal match input
        // 2. Decrement all NoteContent.ResponseOrdinal where NoteFileID, and NoteOrdinal match input and NoteContent.ResponseOrdinal > nc.ResponseOrdinal
        // 3. Decrement single row (Responses field)in BaseNoteHeader where NoteFileID, NoteOrdinal match input
        private static async Task<bool> DeleteResponse(NotesDbContext db, NoteHeader nc)
        {
            int fileId = nc.NoteFileId;
            int arcId = nc.ArchiveId;
            int noteOrd = nc.NoteOrdinal;
            int respOrd = nc.ResponseOrdinal;

            try
            {
                List<NoteHeader> deleteCont = await db.NoteHeader
                    .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal == nc.ResponseOrdinal)
                    .ToListAsync();

                if (deleteCont.Count != 1)
                    return false;

                await DeleteLinked(db, deleteCont.First());

                db.NoteHeader.Remove(deleteCont.First());

                List<NoteHeader> upCont = await db.NoteHeader
                    .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal > respOrd)
                    .ToListAsync();

                foreach (var cont in upCont)
                {
                    cont.ResponseOrdinal--;
                    db.Entry(cont).State = EntityState.Modified;
                }

                NoteHeader bnh = await GetBaseNoteHeader(db, fileId, arcId, noteOrd);

                bnh.ResponseCount--;
                db.Entry(bnh).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }


        public static async Task<string> DeleteLinked(NotesDbContext db, NoteHeader nh)
        {
            // Check for linked notefile(s)

            List<LinkedFile> links = await db.LinkedFile.Where(p => p.HomeFileId == nh.NoteFileId).ToListAsync();

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
                            Activity = LinkAction.Delete,
                            LinkGuid = nh.LinkGuid,
                            LinkedFileId = nh.NoteFileId,
                            BaseUri = link.RemoteBaseUri,
                            Secret = link.Secret
                        };

                        db.LinkQueue.Add(q);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return "Ok";
        }


        public static async Task<NoteHeader> EditNote(NotesDbContext db, UserManager<IdentityUser> userManager, NoteHeader nh, NoteContent nc, string tags)
        {
            NoteHeader eHeader = await GetBaseNoteHeader(db, nh.Id);
            eHeader.LastEdited = nh.LastEdited;
            eHeader.ThreadLastEdited = nh.ThreadLastEdited;
            eHeader.NoteSubject = nh.NoteSubject;
            db.Entry(eHeader).State = EntityState.Modified;

            NoteContent eContent = await GetNoteContent(db, nh.NoteFileId, nh.ArchiveId, nh.NoteOrdinal, nh.ResponseOrdinal);
            eContent.NoteBody = nc.NoteBody;
            eContent.DirectorMessage = nc.DirectorMessage;
            db.Entry(eContent).State = EntityState.Modified;

            List<Tags> oTags = await GetNoteTags(db, nh.NoteFileId, nh.ArchiveId, nh.NoteOrdinal, nh.ResponseOrdinal, 0);
            db.Tags.RemoveRange(oTags);

            db.UpdateRange(oTags);
            db.Update(eHeader);
            db.Update(eContent);

            await db.SaveChangesAsync();

            // deal with tags

            if (tags != null && tags.Length > 1)
            {
                var theTags = Tags.StringToList(tags, eHeader.Id, eHeader.NoteFileId, eHeader.ArchiveId);

                if (theTags.Count > 0)
                {
                    await db.Tags.AddRangeAsync(theTags);
                    await db.SaveChangesAsync();
                }
            }

            // Check for linked notefile(s)

            List<LinkedFile> links = await db.LinkedFile.Where(p => p.HomeFileId == eHeader.NoteFileId && p.SendTo).ToListAsync();

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
                            LinkGuid = eHeader.LinkGuid,
                            LinkedFileId = eHeader.NoteFileId,
                            BaseUri = link.RemoteBaseUri,
                            Secret = link.Secret
                        };

                        db.LinkQueue.Add(q);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return eHeader;
        }

        public static async Task<NoteContent> GetNoteContent(NotesDbContext db, int nfid, int ArcId, int noteord, int respOrd)
        {
            var header = await db.NoteHeader
                .Where(p => p.NoteFileId == nfid && p.ArchiveId == ArcId && p.NoteOrdinal == noteord && p.ResponseOrdinal == respOrd)
                .FirstAsync();

            if (header == null)
                return null;

            var content = await db.NoteContent
                .OfType<NoteContent>()
                .Where(p => p.NoteHeaderId == header.Id)
                .FirstAsync();

            content.NoteHeader = null;

            return content;
        }


        public static UserData GetUserData(UserManager<IdentityUser> userManager, ClaimsPrincipal user, NotesDbContext db)
        {
            UserData aux = null;
            try
            {
                IdentityUser me = userManager.FindByNameAsync(user.Identity.Name).GetAwaiter().GetResult();
                aux = db.UserData.SingleOrDefault(p => p.UserId == me.Id);
            }
            catch
            { }
            return aux;
        }

        public static string GetUserDisplayName(UserManager<IdentityUser> userManager, ClaimsPrincipal user, NotesDbContext db)
        {
            UserData aux = null;
            string myName = " ";
            try
            {
                string userid = userManager.GetUserId(user);
                aux = db.UserData.SingleOrDefault(p => p.UserId == userid);
                myName = aux.DisplayName;
            }
            catch
            { }

            return myName;
        }


        public static string GetSafeUserDisplayName(UserManager<IdentityUser> userManager, ClaimsPrincipal user, NotesDbContext db)
        {
            string uName = GetUserDisplayName(userManager, user, db);
            return uName.Replace(" ", "_");
        }

        public static async Task<Search> GetUserSearch(NotesDbContext db, string userid)
        {
            return await db.Search
                .Where(p => p.UserId == userid)
                .FirstOrDefaultAsync();
        }

        public static async Task<NoteHeader> GetNoteById(NotesDbContext db, long noteid)
        {
            return await db.NoteHeader
                .Include("NoteContent")
                .Include("Tags")
                .Where(p => p.Id == noteid)
                .FirstOrDefaultAsync();
        }

        public static async Task<NoteFile> GetFileByName(NotesDbContext db, string fname)
        {
            return await db.NoteFile
                .Where(p => p.NoteFileName == fname)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<NoteFile>> GetNoteFilesOrderedByNameWithOwner(NotesDbContext db)
        {
            return await db.NoteFile
                .Include(a => a.Owner)
                .OrderBy(p => p.NoteFileName)
                .ToListAsync();
        }


        public static async Task<NoteFile> GetFileByIdWithOwner(NotesDbContext db, int id)
        {
            return await db.NoteFile
                .Include(a => a.Owner)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get next available BaseNoteOrdinal
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="noteFileId">NoteFileID</param>
        /// <returns></returns>
        public static async Task<int> NextBaseNoteOrdinal(NotesDbContext db, int noteFileId, int arcId)
        {
            IOrderedQueryable<NoteHeader> bnhq = GetBaseNoteHeaderByIdRev(db, noteFileId, arcId);

            if (bnhq == null || !bnhq.Any())
                return 1;

            NoteHeader bnh = await bnhq.FirstAsync();
            return bnh.NoteOrdinal + 1;
        }



        public static async Task<long> GetNumberOfNotes(NotesDbContext db, int fileid, int arcId)
        {
            List<NoteHeader> notes = await db.NoteHeader
                                .Where(p => p.NoteFileId == fileid && p.ArchiveId == arcId)
                                .ToListAsync();
            return notes.Count;
        }

        // ReSharper disable once UnusedMember.Global
        public static async Task<long> GetNumberOfBaseNotes(NotesDbContext db, int fileid, int arcId)
        {
            List<NoteHeader> notes = await db.NoteHeader
                                .Where(p => p.Id == fileid && p.ArchiveId == arcId && p.ResponseOrdinal == 0)
                                .ToListAsync();
            return notes.Count;
        }

        /// <summary>
        /// Get BaseNoteHeaders in reverse order - we only plan to look at the 
        /// first one/one with highest NoteOrdinal
        /// </summary>
        /// <param name="db"></param>
        /// <param name="noteFileId"></param>
        /// <returns></returns>
        private static IOrderedQueryable<NoteHeader> GetBaseNoteHeaderByIdRev(NotesDbContext db, int noteFileId, int arcId)
        {
            return db.NoteHeader
                            .Where(p => p.NoteFileId == noteFileId && p.ArchiveId == arcId && p.ResponseOrdinal == 0)
                            .OrderByDescending(p => p.NoteOrdinal);
        }

        public static async Task<NoteFile> GetFileById(NotesDbContext db, int id)
        {
            return await db.NoteFile
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public static async Task<NoteHeader> GetNoteHeader(NotesDbContext db, long id)
        {
            return await db.NoteHeader
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

        }

        public static async Task<List<NoteHeader>> GetBaseNoteHeaders(NotesDbContext db, int id, int arcId)
        {
            return await db.NoteHeader
                .Where(p => p.NoteFileId == id && p.ArchiveId == arcId && p.ResponseOrdinal == 0)
                .OrderBy(p => p.NoteOrdinal)
                .ToListAsync();
        }

        public static async Task<List<NoteHeader>> GetBaseNoteAndResponses(NotesDbContext db, int nfid, int arcId, int noteord)
        {
            return await db.NoteHeader
                .Include("NoteContent")
                .Include("Tags")
                .Where(p => p.NoteFileId == nfid && p.ArchiveId == arcId && p.NoteOrdinal == noteord)
                .ToListAsync();
        }

        public static async Task<NoteHeader> GetNoteByIdWithFile(NotesDbContext db, long noteid)
        {
            return await db.NoteHeader
                .Include("NoteContent")
                .Include("NoteFile")
                .Include("Tags")
                .Where(p => p.Id == noteid)
                .OrderBy((x => x.NoteOrdinal))
                .FirstOrDefaultAsync();

        }

        public static async Task<NoteHeader> GetBaseNoteHeader(NotesDbContext db, long id)
        {
            NoteHeader nh = await db.NoteHeader
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            return await db.NoteHeader
                .Where(p => p.Id == nh.BaseNoteId)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<NoteHeader>> GetBaseNoteAndResponsesHeaders(NotesDbContext db, int nfid, int arcId, int noteord)
        {
            return await db.NoteHeader
                .Where(p => p.NoteFileId == nfid && p.ArchiveId == arcId && p.NoteOrdinal == noteord)
                .ToListAsync();
        }

        public static async Task<List<Tags>> GetNoteTags(NotesDbContext db, int nfid, int arcId, int noteord, int respOrd, int dummy)
        {
            var header = await db.NoteHeader
                .Where(p => p.NoteFileId == nfid && p.ArchiveId == arcId && p.NoteOrdinal == noteord && p.ResponseOrdinal == respOrd)
                .FirstAsync();

            if (header == null)
                return null;

            var tags = await db.Tags
                .Where(p => p.NoteHeaderId == header.Id)
                .ToListAsync();

            foreach (var tag in tags)
            {
                tag.NoteHeader = null;
            }

            return tags;
        }


        //TODO
        //public static async Task<bool> SendNotesAsync(ForwardViewModel fv, NotesDbContext db, IEmailSender emailSender,
        //        string email, string name, string Url)
        //{
        //    await emailSender.SendEmailAsync(fv.ToEmail, fv.NoteSubject,
        //        await MakeNoteForEmail(fv, db, email, name, Url));

        //    return true;
        //}


        private static async Task<string> MakeNoteForEmail(ForwardViewModel fv, NotesDbContext db, string email, string name, string ProductionUrl)
        {
            NoteHeader nc = await GetNoteByIdWithFile(db, fv.NoteID);

            if (!fv.hasstring || !fv.wholestring)
            {
                return "Forwarded by Notes 2021 - User: " + email + " / " + name
                    + "<p>File: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p><hr/>"
                    + "<p>Author: " + nc.AuthorName + "  - Director Message: " + nc.NoteContent.DirectorMessage + "</p><p>"
                    + "<p>Subject: " + nc.NoteSubject + "</p>"
                    + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " UTC" + "</p>"
                    + nc.NoteContent.NoteBody
                    + "<hr/>" + "<a href=\"" + ProductionUrl + "NoteDisplay/Display/" + fv.NoteID + "\" >Link to note</a>";
            }
            else
            {
                List<NoteHeader> bnhl = await GetBaseNoteHeadersForNote(db, nc.NoteFileId, nc.ArchiveId, nc.NoteOrdinal);
                NoteHeader bnh = bnhl[0];
                fv.NoteSubject = bnh.NoteSubject;
                List<NoteHeader> notes = await GetBaseNoteAndResponses(db, nc.NoteFileId, nc.ArchiveId, nc.NoteOrdinal);

                StringBuilder sb = new StringBuilder();
                sb.Append("Forwarded by Notes 2020 - User: " + email + " / " + name
                    + "<p>\nFile: " + nc.NoteFile.NoteFileName + " - File Title: " + nc.NoteFile.NoteFileTitle + "</p>"
                    + "<hr/>");

                for (int i = 0; i < notes.Count; i++)
                {
                    if (i == 0)
                    {
                        sb.Append("<p>Base Note - " + (notes.Count - 1) + " Response(s)</p>");
                    }
                    else
                    {
                        sb.Append("<hr/><p>Response - " + notes[i].ResponseOrdinal + " of " + (notes.Count - 1) + "</p>");
                    }
                    sb.Append("<p>Author: " + notes[i].AuthorName + "  - Director Message: " + notes[i].NoteContent.DirectorMessage + "</p>");
                    sb.Append("<p>Subject: " + notes[i].NoteSubject + "</p>");
                    sb.Append("<p>" + notes[i].LastEdited.ToShortDateString() + " " + notes[i].LastEdited.ToShortTimeString() + " UTC" + " </p>");
                    sb.Append(notes[i].NoteContent.NoteBody);
                    sb.Append("<hr/>");
                    sb.Append("<a href=\"");
                    sb.Append(ProductionUrl + "NoteDisplay/Display/" + notes[i].Id + "\" >Link to note</a>");
                }

                return sb.ToString();
            }

        }

        /// <summary>
        /// Get the BaseNoteHeader for a Note
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="nfid">fileid</param>
        /// <param name="noteord"></param>
        /// <returns></returns>
        public static async Task<List<NoteHeader>> GetBaseNoteHeadersForNote(NotesDbContext db, int nfid, int arcId, int noteord)
        {
            return await db.NoteHeader
                .Where(p => p.NoteFileId == nfid && p.ArchiveId == arcId && p.NoteOrdinal == noteord && p.ResponseOrdinal == 0)
                .ToListAsync();
        }

        public static async Task<NoteHeader> GetBaseNoteHeaderForOrdinal(NotesDbContext db, int fileid, int arcId, int ord)
        {
            return await db.NoteHeader
                .Where(p => p.NoteFileId == fileid && p.ArchiveId == arcId && p.NoteOrdinal == ord && p.ResponseOrdinal == 0)
                .FirstOrDefaultAsync();
        }


        public static async Task<NoteHeader> GetEditedNoteHeader(NotesDbContext db, NoteHeader edited)
        {
            return await db.NoteHeader
                .Where(p => p.NoteFileId == edited.NoteFileId && p.ArchiveId == edited.ArchiveId && p.NoteOrdinal == edited.NoteOrdinal)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Given a NoteContent Object and Response number get the response NoteID
        /// </summary>
        /// <param name="db"></param>
        /// <param name="nc"></param>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static async Task<long?> FindResponseId(NotesDbContext db, NoteHeader nc, int resp)
        {
            NoteHeader content = await db.NoteHeader
                .Where(p => p.NoteFileId == nc.NoteFileId && p.ArchiveId == nc.ArchiveId && p.NoteOrdinal == nc.NoteOrdinal && p.ResponseOrdinal == resp)
                .FirstOrDefaultAsync();

            return content?.Id;
        }

        public static async Task<NoteFile> GetFileByIdWithHeaders(NotesDbContext db, int id, int arcId)
        {
            NoteFile nf = await db.NoteFile
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            //nf.NoteHeaders = await db.NoteHeader.Where(p => p.NoteFileId == id && p.ArchiveId == arcId).ToListAsync();

            return nf;
        }

        public static async Task<List<NoteHeader>> GetAllHeaders(NotesDbContext db, int id, int arcId)
        {
            return await db.NoteHeader.Where(p => p.NoteFileId == id && p.ArchiveId == arcId).ToListAsync();
        }

        /// <summary>
        /// Get the BaseNoteHeader in a given file with given ordinal
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="fileId">fileid</param>
        /// <param name="noteOrd">noteordinal</param>
        /// <returns></returns>
        public static async Task<NoteHeader> GetBaseNoteHeader(NotesDbContext db, int fileId, int arcId, int noteOrd)
        {
            return await db.NoteHeader
                                .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal == noteOrd && p.ResponseOrdinal == 0)
                                .FirstOrDefaultAsync();
        }



        /// <summary>
        /// Get a list of all Notes in a string/thread
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="fileId">fileid</param>
        /// <param name="noteOrd">NoteOrdinal - identifies the string/thread</param>
        /// <returns></returns>
        private static async Task<List<NoteHeader>> GetNoteContentList(NotesDbContext db, int fileId, int arcId, int noteOrd)
        {
            return await db.NoteHeader
                .Where(p => p.NoteFileId == fileId && p.ArchiveId == arcId && p.NoteOrdinal == noteOrd)
                .ToListAsync();
        }

        public static async Task<List<NoteHeader>> GetSearchResponseList(NotesDbContext db, Search start, int myRespOrdinal, NoteHeader bnh, SearchOption so)
        {
            // First try responses
            if (so == SearchOption.Tag)
            {
                return await db.NoteHeader
                    .Include("Tags")
                    .Where(x => x.NoteFileId == start.NoteFileId && x.ArchiveId == start.ArchiveId && x.NoteOrdinal == bnh.NoteOrdinal && x.ResponseOrdinal > myRespOrdinal)
                    .ToListAsync();

            }
            if (so == SearchOption.Content || so == SearchOption.DirMess)
            {
                return await db.NoteHeader
                .Include("NoteContent")
                .Where(x => x.NoteFileId == start.NoteFileId && x.ArchiveId == start.ArchiveId && x.NoteOrdinal == bnh.NoteOrdinal && x.ResponseOrdinal > myRespOrdinal)
                .ToListAsync();
            }

            return await db.NoteHeader
            .Where(x => x.NoteFileId == start.NoteFileId && x.ArchiveId == start.ArchiveId && x.NoteOrdinal == bnh.NoteOrdinal && x.ResponseOrdinal > myRespOrdinal)
            .ToListAsync();
        }

        public static async Task<List<NoteHeader>> GetSearchHeaders(NotesDbContext db, Search start, NoteHeader bnh, SearchOption so)
        {
            if (so == SearchOption.Tag)
            {
                return await db.NoteHeader
                .Include("Tags")
                .Where(x => x.NoteFileId == start.NoteFileId && x.ArchiveId == start.ArchiveId && x.NoteOrdinal > bnh.NoteOrdinal)
                .ToListAsync();
            }
            if (so == SearchOption.Content || so == SearchOption.DirMess)
            {
                return await db.NoteHeader
                .Include("NoteContent")
                .Where(x => x.NoteFileId == start.NoteFileId && x.ArchiveId == start.ArchiveId && x.NoteOrdinal > bnh.NoteOrdinal)
                .ToListAsync();
            }
            return await db.NoteHeader
                .Where(x => x.NoteFileId == start.NoteFileId && x.ArchiveId == start.ArchiveId && x.NoteOrdinal > bnh.NoteOrdinal)
                .ToListAsync();

        }

        public static async Task<List<Sequencer>> GetSeqListForUser(NotesDbContext db, string userid)
        {
            return await db.Sequencer
                .Where(x => x.UserId == userid)
                .OrderBy(x => x.Ordinal)
                .ToListAsync();
        }

        public static async Task<List<NoteHeader>> GetSbnh(NotesDbContext db, Sequencer myseqfile)
        {
            return await db.NoteHeader
                            .Where(x => x.NoteFileId == myseqfile.NoteFileId && x.ArchiveId == 0
                            && x.ResponseOrdinal == 0
                            && x.ThreadLastEdited >= myseqfile.LastTime)
                            .OrderBy(x => x.NoteOrdinal)
                            .ToListAsync();
        }

        public static async Task<List<NoteHeader>> GetSeqHeader1(NotesDbContext db, Sequencer myseqfile, NoteHeader bnh)
        {
            return await db.NoteHeader
                .Where(x => x.NoteFileId == myseqfile.NoteFileId && x.ArchiveId == 0
                    && x.LastEdited >= myseqfile.LastTime && x.NoteOrdinal > bnh.NoteOrdinal && x.ResponseOrdinal == 0)
                .OrderBy(x => x.NoteOrdinal)
                .ToListAsync();
        }

        public static async Task<List<NoteHeader>> GetSeqHeader2(NotesDbContext db, Sequencer myseqfile)
        {
            return await db.NoteHeader
                .Where(x => x.NoteFileId == myseqfile.NoteFileId && x.ArchiveId == 0 && x.LastEdited >= myseqfile.LastTime && x.ResponseOrdinal == 0)
                .OrderBy(x => x.NoteOrdinal)
                .ToListAsync();
        }

        /// <summary>
        /// Get all the BaseNoteHeaders for a file
        /// </summary>
        /// <param name="db">NotesDbContext</param>
        /// <param name="nfid">fileid</param>
        /// <returns></returns>
        public static async Task<List<NoteHeader>> GetBaseNoteHeadersForFile(NotesDbContext db, int nfid, int arcId)
        {
            return await db.NoteHeader
                .Where(p => p.NoteFileId == nfid && p.ArchiveId == arcId && p.ResponseOrdinal == 0)
                .OrderBy(p => p.NoteOrdinal)
                .ToListAsync();
        }

        public static async Task<List<NoteHeader>> GetOrderedListOfResponses(NotesDbContext db, int nfid, NoteHeader bnh)
        {
            return await db.NoteHeader
                .Include(m => m.NoteContent)
                .Include(m => m.Tags)
                .Where(p => p.NoteFileId == nfid && p.ArchiveId == bnh.ArchiveId && p.NoteOrdinal == bnh.NoteOrdinal && p.ResponseOrdinal > 0)
                .OrderBy(p => p.ResponseOrdinal)
                .ToListAsync();
        }

        public static async Task<NoteHeader> GetMarkedNote(NotesDbContext db, Mark mark)
        {
            return await db.NoteHeader
                .Include(m => m.NoteContent)
                .Include(m => m.Tags)
                .Where(p => p.NoteFileId == mark.NoteFileId && p.ArchiveId == mark.ArchiveId && p.NoteOrdinal == mark.NoteOrdinal && p.ResponseOrdinal == mark.ResponseOrdinal)
                .FirstAsync();
        }

        public static async Task<NoteHeader> GetBaseNoteHeaderById(NotesDbContext db, long id)
        {
            return await db.NoteHeader
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public static async Task<List<NoteFile>> GetNoteFilesOrderedByName(NotesDbContext db)
        {
            return await db.NoteFile
                .OrderBy(p => p.NoteFileName)
                .ToListAsync();
        }


    }

}
