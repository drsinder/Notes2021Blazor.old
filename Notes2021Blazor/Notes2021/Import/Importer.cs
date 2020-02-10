/*--------------------------------------------------------------------------
**
**  Copyright (c) 2020, Dale Sinder
**
**  Name: Import.cs
**
**  Description:
**      Notes Import for Notes 2021
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


using Notes2021Blazor.Shared;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Notes2021.Import
{
    public partial class Importer
    {
        private const char Ff = (char)(12); //  FF

        public async Task<bool> Import(NotesDbContext _db, string myFile, string myNotes)
        {
            Output("");

            StreamReader file;
            try
            {
                file = new StreamReader(myFile);
            }
            catch
            {
                return false;
            }

            NoteFile noteFile = await NoteDataManager.GetFileByName(_db, myNotes);


            //int id = noteFile.Id;
            int numberArchives = noteFile.NumberArchives;
            long counter = 0;
            bool isResp = false;
            char[] spaceTrim = new char[] { ' ' };
            char[] slash = new char[] { '/' };
            char[] colon = new char[] { ':' };
            char[] dash = new char[] { '-' };

            string platoBaseYear = "";

            StringBuilder sb = new StringBuilder();
            long baseNoteHeaderId = 0;
            NoteContent newContent = null;
            NoteHeader makeHeader = null;
            int basenotes = 0;

            int filetype = 0;  // 0= NovaNET | 1 = Notes 3.1 | 2 = plato iv group notes -- we can process three formats


            // Read the file and process it line by line.
            //try
            {
                NoteHeader bnh;
                string line;
                NoteHeader newHeader;
                while ((line = await file.ReadLineAsync()) != null)
                {

                    if (counter == 0)
                    {
                        if (line.StartsWith("2021 NoteFile "))  // By this we know it came from Notes 3.1
                        {
                            filetype = 1;   // Notes 3.1
                            await file.ReadLineAsync();
                            line = await file.ReadLineAsync();
                        }
                        else if (line.StartsWith("+++ plato iv group notes +++"))
                        {
                            filetype = 2;   // plato iv group notes
                            await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            var platoLine5 = await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            await file.ReadLineAsync();

                            await file.ReadLineAsync();
                            await file.ReadLineAsync();
                            line = await file.ReadLineAsync();

                            string[] splits = platoLine5.Split(spaceTrim);
                            platoBaseYear = splits[splits.Length - 1];

                        }
                    }

                    if (filetype == 0)  // Process for NovaNET output
                    {

                        line = await CheckFf(line, file);
                        if (line.Length > 52)  // Possible Note Header
                        {
                            string head = line.Substring(46);
                            if (head.StartsWith("  Note ")) //new note found
                            {
                                if (newContent != null)  // have a note to write
                                {
                                    newContent.NoteBody = sb.ToString();
                                    sb = new StringBuilder();
                                    newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                                    if (!isResp) // base note
                                    {
                                        basenotes++;
                                        newHeader = await NoteDataManager.CreateNote(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                                        NoteHeader baseNoteHeader = await GetBaseNoteHeader(_db, newHeader);
                                        baseNoteHeaderId = baseNoteHeader.BaseNoteId;

                                        //baseNoteHeader.CreateDate = newHeader.LastEdited;
                                        //baseNoteHeader.ThreadLastEdited = newHeader.LastEdited;
                                        //_db.Entry(baseNoteHeader).State = EntityState.Modified;
                                        //await _db.SaveChangesAsync();

                                        if (basenotes % 10 == 0)
                                        {
                                            Output("Base notes: " + basenotes);
                                            if (basenotes % 100 == 0)
                                            {
                                                GC.Collect();
                                                GC.WaitForPendingFinalizers();
                                            }
                                        }

                                    }
                                    else // resp
                                    {
                                        bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                                        makeHeader.NoteOrdinal = bnh.NoteOrdinal;
                                        makeHeader.BaseNoteId = bnh.Id;  //Fix
                                        await NoteDataManager.CreateResponse(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                                    }
                                }
                                //newContent.Dispose();
                                //makeHeader.Dispose();
                                newContent = new NoteContent();
                                makeHeader = new NoteHeader { NoteFileId = noteFile.Id };

                                // get title at start of line
                                var title = line.Substring(0, 40).TrimEnd(spaceTrim);
                                makeHeader.NoteSubject = title;
                                isResp = head.Contains("Response");  // is this a response?

                                line = file.ReadLine();     // get next line  
                                line = await CheckFf(line, file);
                                if (isResp && line.StartsWith("----")) // perhaps a response title
                                {
                                    title = line.Substring(4, line.Length - 4).TrimEnd(spaceTrim);
                                    makeHeader.NoteSubject = title;
                                    line = await file.ReadLineAsync();  // skip line
                                    await CheckFf(line, file);
                                }
                                else if (isResp)
                                    makeHeader.NoteSubject = "*none*";  // must have a title

                                if (string.IsNullOrEmpty(makeHeader.NoteSubject))  // must have a title
                                    makeHeader.NoteSubject = "*none*";

                                line = await file.ReadLineAsync();  // skip line
                                line = await CheckFf(line, file);

                                // Check for possible director message 
                                if (line.StartsWith("    "))
                                {
                                    newContent.DirectorMessage = line.Trim(spaceTrim);
                                    line = await file.ReadLineAsync();  // skip line
                                    await CheckFf(line, file);
                                    line = await file.ReadLineAsync();  // skip line
                                    line = await CheckFf(line, file);
                                }


                                // get date, time, author
                                // first date
                                string date = line.Substring(0, 10).TrimEnd(spaceTrim);
                                string[] x = date.Split(slash);

                                string prefix = "/20";
                                int yearpart = int.Parse(x[2]);
                                if (yearpart > 70)
                                    prefix = "/19";

                                string datetime = (x[0].Length == 1 ? "0" + x[0] : x[0]) + "/" + (x[1].Length == 1 ? "0" + x[1] : x[1]) + prefix + x[2];

                                // now time
                                string time = line.Substring(10, 6);
                                time = time.Trim(spaceTrim);
                                time = time.Replace("am", " ");
                                time = time.Replace("pm", " ");
                                time = time.Replace("a", " ");
                                time = time.Replace("p", " ");
                                time = time.TrimEnd(spaceTrim);

                                string[] y = time.Split(colon);
                                int hour = int.Parse(y[0]);
                                if (line.Substring(0, 23).Contains("pm") && hour < 12)
                                    hour = hour + 12;

                                datetime += " " + ((hour < 10) ? "0" + hour.ToString() : hour.ToString()) + ":" + y[1];

                                // Save 
                                makeHeader.LastEdited = DateTime.Parse(datetime);

                                //nc.LastEdited = tzone.Universal(nc.LastEdited);

                                // author
                                makeHeader.AuthorName = line.Substring(25).Trim(spaceTrim);
                                makeHeader.AuthorID = Globals.ImportedAuthorId();   //"imported";
                                line = await file.ReadLineAsync();  // skip line
                                line = await CheckFf(line, file);
                            }
                        }
                        // append line to current note
                        sb.AppendLine(line);
                        counter++;
                    }  // end NovaNET

                    else if (filetype == 1)  // Process from Notes 3.1 export as text
                    {
                        if (line.StartsWith("Note: "))  // possible note header
                        {
                            string[] parts = line.Split(dash);
                            if (parts.Length >= 5)  // looks like the right number of sections > with - in subject grrr
                            {
                                if (newContent != null)  // have a note to write
                                {
                                    sb.Append(" ");
                                    newContent.NoteBody = sb.ToString();
                                    sb = new StringBuilder();
                                    //nc.NoteBody = nc.NoteBody.Replace("\r\n", "<br />");

                                    if (!isResp) // base note
                                    {
                                        basenotes++;
                                        newHeader = await NoteDataManager.CreateNote(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                                        NoteHeader baseNoteHeader = await GetBaseNoteHeader(_db, newHeader);
                                        baseNoteHeaderId = baseNoteHeader.BaseNoteId;

                                        //baseNoteHeader.CreateDate = newHeader.LastEdited;
                                        //baseNoteHeader.ThreadLastEdited = newHeader.LastEdited;
                                        //_db.Entry(baseNoteHeader).State = EntityState.Modified;
                                        //await _db.SaveChangesAsync();

                                        if (basenotes % 10 == 0)
                                        {
                                            Output("Base notes: " + basenotes);
                                            if (basenotes % 100 == 0)
                                            {
                                                GC.Collect();
                                                GC.WaitForPendingFinalizers();
                                            }
                                        }


                                    }
                                    else // resp
                                    {
                                        bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                                        makeHeader.NoteOrdinal = bnh.NoteOrdinal;
                                        makeHeader.BaseNoteId = bnh.Id;  //Fix
                                        await NoteDataManager.CreateResponse(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                                    }
                                }
                                //newContent.Dispose();
                                //makeHeader.Dispose();
                                newContent = new NoteContent();
                                makeHeader = new NoteHeader { NoteFileId = noteFile.Id };

                                // parse sections, 0 is note number, 1 is subject, 2 is author, 3 is datetime, 4 is number of responses
                                // skip section 0
                                // Get subject

                                string part = parts[1];
                                if (part.StartsWith(" Subject: "))
                                {
                                    if (parts.Length == 5)
                                        makeHeader.NoteSubject = part.Substring(9, part.Length - 9).Trim() + " ";  //only works if no - in subject grrr
                                    else
                                    {
                                        int subjectindx = line.IndexOf(" - Subject: ", StringComparison.Ordinal);
                                        int authorindx = line.IndexOf(" - Author: ", StringComparison.Ordinal);
                                        makeHeader.NoteSubject = subjectindx < authorindx ?
                                            line.Substring(subjectindx + 12, authorindx - subjectindx - 12).Trim()
                                            : "** Subject Parse Error **";
                                    }
                                }
                                // get author
                                part = parts[parts.Length - 3];
                                if (part.StartsWith(" Author: "))
                                {
                                    makeHeader.AuthorName = part.Substring(8, part.Length - 8).Trim();
                                    makeHeader.AuthorID = Globals.ImportedAuthorId();
                                }
                                else
                                {
                                    makeHeader.AuthorName = "** Author Parse Error **";
                                    makeHeader.AuthorID = Globals.ImportedAuthorId();
                                }
                                part = parts[parts.Length - 2].Trim();
                                makeHeader.LastEdited = DateTime.Parse(part);
                                // skip resp

                                line = await file.ReadLineAsync();
                                if (line.StartsWith("Tags -"))
                                {
                                    isResp = false;
                                    newContent.DirectorMessage = line.Substring(6, line.Length - 6).Trim();
                                }
                                else if (line.StartsWith("Base Note Subject: "))
                                {
                                    isResp = true;  // but skip content
                                    line = await file.ReadLineAsync();  // Should be Tag
                                    if (line.StartsWith("Tags -"))
                                    {
                                        newContent.DirectorMessage = line.Substring(6, line.Length - 6).Trim();
                                    }
                                }
                                await file.ReadLineAsync();  //skip a line
                                line = await file.ReadLineAsync();  //first content line
                            }
                        }
                        // append line to current note
                        sb.AppendLine(line);
                        counter++;
                    }  // end Notes 3.1

                    else if (filetype == 2)  // PLATO iv group notes
                    {

                        int xflag = 0;
                        if (line.Contains("-------- note "))  //  note header
                            xflag = 1;
                        else if (line.Contains("-------- response "))  //  response header
                            xflag = 2;

                        if (xflag > 0) // we have note to write (maybe)
                        {
                            if (newContent != null)  // have a note to write
                            {
                                newContent.NoteBody = sb.ToString();
                                sb = new StringBuilder();
                                newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                                if (!isResp) // base note
                                {
                                    basenotes++;
                                    newHeader = await NoteDataManager.CreateNote(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                                    NoteHeader baseNoteHeader = await GetBaseNoteHeader(_db, newHeader);
                                    baseNoteHeaderId = baseNoteHeader.BaseNoteId;

                                    //baseNoteHeader.CreateDate = newHeader.LastEdited;
                                    //baseNoteHeader.ThreadLastEdited = newHeader.LastEdited;
                                    //_db.Entry(baseNoteHeader).State = EntityState.Modified;
                                    //await _db.SaveChangesAsync();

                                    if (basenotes % 10 == 0)
                                    {
                                        Output("Base notes: " + basenotes);
                                        if (basenotes % 100 == 0)
                                        {
                                            GC.Collect();
                                            GC.WaitForPendingFinalizers();
                                        }
                                    }

                                }
                                else // resp
                                {
                                    bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                                    makeHeader.NoteOrdinal = bnh.NoteOrdinal;
                                    makeHeader.BaseNoteId = bnh.Id;  //Fix
                                    await NoteDataManager.CreateResponse(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                                }
                            }
                            //newContent.Dispose();
                            //makeHeader.Dispose();
                            newContent = new NoteContent();
                            makeHeader = new NoteHeader { NoteFileId = noteFile.Id };

                            // now start with new note proc

                            if (xflag == 1) // get note title
                            {
                                // mark we have a base note / get title

                                line = line.Substring(16);
                                string[] splitsx = line.Split(spaceTrim);
                                makeHeader.NoteSubject = line.Substring(splitsx[0].Length).Trim(spaceTrim);

                                isResp = false;
                            }
                            else if (xflag == 2)
                            {
                                isResp = true;  // mark response
                                makeHeader.NoteSubject = "*none*";  // must have a title
                            }
                            if (string.IsNullOrEmpty(makeHeader.NoteSubject))  // must have a title
                                makeHeader.NoteSubject = "*none*";

                            line = await file.ReadLineAsync(); // move to info header

                            // process header

                            // count the /s to get date format.

                            int cnt = Regex.Matches(line, "/").Count;
                            if (cnt < 1) // try next line grrr.
                            {
                                line = await file.ReadLineAsync(); // move to info header
                                cnt = Regex.Matches(line, "/").Count;
                            }

                            line = " " + line;
                            line = line.Replace("     ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");
                            line = line.Replace("     ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");


                            string[] splits = line.Split(' ', '/', '.', ':', ',', ';');

                            string date;
                            if (cnt == 1)
                            {
                                date = splits[1] + "/" + splits[2] + "/" + platoBaseYear + " "
                                    + splits[3] + ":" + splits[4] + ":00";
                            }
                            else
                            {
                                date = splits[1] + "/" + splits[2] + "/19" + splits[3] + " "
                                    + splits[4] + ":" + splits[5] + ":00";
                            }
                            makeHeader.LastEdited = DateTime.Parse(date);

                            string group = " " + splits[splits.Length - 1];
                            string name = "";
                            for (int i = 4 + cnt; i < splits.Length - 1; i++)
                            {
                                name += splits[i] + " ";
                            }

                            makeHeader.AuthorName = name + "/" + group;
                            makeHeader.AuthorID = Globals.ImportedAuthorId();   //"imported";

                            await file.ReadLineAsync(); // skip lines to get to content
                            line = await file.ReadLineAsync(); // skip lines to get to content
                        }

                        //// pre proc above
                        sb.AppendLine(line); // collect lines of note
                        counter++;
                    }  // end PLATO
                }  // end where


                if (filetype == 0)  // NovaNET
                {
                    if (newContent != null)  // have a note to write
                    {
                        newContent.NoteBody = sb.ToString();
                        sb.Clear();

                        newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                        if (!isResp) // base note
                        {
                            //basenotes++;
                            //TODO
                            //newHeader = await NoteDataManager.CreateNote(_db, _userManager, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false);
                            //NoteHeader baseNoteHeader = await GetBaseNoteHeader(newHeader);
                            //baseNoteHeaderId = baseNoteHeader.BaseNoteId;

                            //baseNoteHeader.CreateDate = newHeader.LastEdited;
                            //baseNoteHeader.ThreadLastEdited = newHeader.LastEdited;
                            //_db.Entry(baseNoteHeader).State = EntityState.Modified;
                            //await _db.SaveChangesAsync();
                        }
                        else // resp
                        {
                            bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                            makeHeader.NoteOrdinal = bnh.NoteOrdinal;
                            makeHeader.BaseNoteId = bnh.Id;  //Fix
                            await NoteDataManager.CreateResponse(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                        }
                    }
                }
                else if (filetype == 1)  // Notes 3.1
                {
                    if (newContent != null)  // have a note to write
                    {
                        sb.Append(" ");
                        newContent.NoteBody = sb.ToString();
                        sb.Clear();

                        if (!isResp) // base note
                        {
                            //basenotes++;
                            // ReSharper disable once RedundantAssignment
                            newHeader = await NoteDataManager.CreateNote(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                            //NoteHeader baseNoteHeader = await GetBaseNoteHeader(newHeader);
                            //baseNoteHeaderId = baseNoteHeader.BaseNoteId;

                            //baseNoteHeader.CreateDate = newHeader.LastEdited;
                            //baseNoteHeader.ThreadLastEdited = newHeader.LastEdited;
                            //_db.Entry(baseNoteHeader).State = EntityState.Modified;
                            //await _db.SaveChangesAsync();
                        }
                        else // resp
                        {
                            bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                            makeHeader.NoteOrdinal = bnh.NoteOrdinal;
                            makeHeader.BaseNoteId = bnh.Id;  //Fix
                            await NoteDataManager.CreateResponse(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                        }
                    }
                }
                else if (filetype == 2)  // PLATO
                {
                    if (newContent != null)  // have a note to write
                    {
                        newContent.NoteBody = sb.ToString();
                        sb.Clear();
                        newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                        if (!isResp) // base note
                        {
                            //basenotes++;
                            // ReSharper disable once RedundantAssignment
                            newHeader = await NoteDataManager.CreateNote(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                            //NoteHeader baseNoteHeader = await GetBaseNoteHeader(newHeader);
                            //baseNoteHeaderId = baseNoteHeader.BaseNoteId;

                            //baseNoteHeader.CreateDate = newHeader.LastEdited;
                            //baseNoteHeader.ThreadLastEdited = newHeader.LastEdited;
                            //_db.Entry(baseNoteHeader).State = EntityState.Modified;
                            //await _db.SaveChangesAsync();
                        }
                        else // resp
                        {
                            bnh = await NoteDataManager.GetBaseNoteHeaderById(_db, baseNoteHeaderId);
                            makeHeader.NoteOrdinal = bnh.NoteOrdinal;
                            makeHeader.BaseNoteId = bnh.Id;  //Fix
                            await NoteDataManager.CreateResponse(_db, null, makeHeader, newContent.NoteBody, string.Empty, newContent.DirectorMessage, false, false);
                        }
                    }

                }

            }
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

            file.Close();

            noteFile.NumberArchives = numberArchives;
            _db.Update(noteFile);
            _db.SaveChanges();

            Output("  Basenotes: " + basenotes + "   Completed!!");


            return true;
        }

        /// <summary>
        /// Process NovaNET formfeed
        /// </summary>
        /// <param name="inline">input line</param>
        /// <param name="file">StreamReader for import file</param>
        /// <returns></returns>
        public async Task<string> CheckFf(string inline, StreamReader file)
        {
            if (inline.Length == 1)
            {
                char[] mychars = inline.ToCharArray();
                if (mychars[0] == Ff)
                {
                    await file.ReadLineAsync();
                    await file.ReadLineAsync();
                    await file.ReadLineAsync();
                    await file.ReadLineAsync();
                    string it = await file.ReadLineAsync();
                    return string.IsNullOrEmpty(it) ? string.Empty : it;
                }
            }
            return inline;
        }


        //public async Task<string> CheckFFPLATO(string inline, StreamReader file)
        //{
        //    string line;
        //    if (inline.Length == 1)
        //    {
        //        char[] mychars = inline.ToCharArray();
        //        if (mychars[0] == FF)
        //        {
        //            line = await file.ReadLineAsync();
        //            line = await file.ReadLineAsync();
        //            line = await file.ReadLineAsync();
        //            return line;
        //        }
        //    }
        //    return inline;
        //}

        /// <summary>
        /// Get the BaseNoteHeader for a NoteContent
        /// </summary>
        /// <param name="nc">NoteContent</param>
        /// <returns></returns>
        public async Task<NoteHeader> GetBaseNoteHeader(NotesDbContext _db, NoteHeader nc)
        {
            return await NoteDataManager.GetBaseNoteHeader(_db, nc.NoteFileId, 0, nc.NoteOrdinal);
        }

        public virtual void Output(string message)
        {
        }
    }



}
