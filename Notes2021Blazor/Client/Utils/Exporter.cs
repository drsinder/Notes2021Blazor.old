using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Notes2021Blazor.Shared;
using System.IO;
using System.Text;
using System.Net;

namespace Notes2021Blazor.Client
{
    public static class Exporter
    {
        public static async Task<MemoryStream> DoExport(ExportViewModel model, WebClient Http, string ProdUri, string stylePath)
        {
            bool isHtml = model.isHtml;
            bool isCollapsible = model.isCollapsible;

            NoteFile nf = model.NoteFile;
            int nfid = nf.Id;

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            StringBuilder sb = new StringBuilder();

            if (isHtml)
            {
                // Start the document
                sb.AppendLine("<!DOCTYPE html>");
                sb.AppendLine("<html>");
                sb.AppendLine("<meta charset=\"utf-8\" />");
                sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                sb.AppendLine("<title>" + nf.NoteFileTitle + "</title>");

                sb.AppendLine("<link rel = \"stylesheet\" href = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css\">");

                sb.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\" ></script >");
                sb.AppendLine("<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js\" ></script >");
                sb.AppendLine("<script src = \"https://notes2020.drsinder.com/js/prism.min.js\" ></script >");

                sb.AppendLine("<style>");

                // read our local style sheet from a file and output it
                TextReader sr = new StreamReader(stylePath);
                sb.AppendLine(await sr.ReadToEndAsync());
                sr.Close();

                sb.AppendLine("</style>");

                sb.AppendLine("</head>");
                sb.AppendLine("<body>");
                await sw.WriteAsync(sb.ToString());

                // ready to start  writing content of file
                sb = new StringBuilder();
            }
            if (isHtml)
                sb.Append("<h2>");

            // File Header
            sb.Append("2021 NoteFile " + nf.NoteFileName + " - " + nf.NoteFileTitle);
            sb.Append(" - Created " + DateTime.Now.ToUniversalTime().ToLongDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
            if (isHtml)
            {
                sb.Append("</h2>");
                sb.Append("<h4>");
                sb.Append("<a href=\"");
                sb.Append(ProdUri + "/NoteDisplay/Create/" + nf.Id +
                          "\" target=\"_blank\">New Base Note</a>");
                sb.Append("</h4>");
            }

            await sw.WriteLineAsync(sb.ToString());
            await sw.WriteLineAsync();

            // get ordered list of basenoteheaders to start process
            List<NoteHeader> bnhl = null;
            string req;
            if (model.NoteOrdinal == 0)
            {
                //bnhl = await NoteDataManager.GetBaseNoteHeadersForFile(_db, nfid, arcId);

                req = "" + nfid + "." + model.ArchiveNumber + ".0";
            }
            else
            {
                //bnhl = await NoteDataManager.GetBaseNoteHeadersForNote(_db, nfid, arcId, model.NoteOrdinal);

                req = "" + nfid + "." + model.ArchiveNumber + "." + model.NoteOrdinal;
            }

            /// get the bnhl with Export

            // loop over each base note in order
            foreach (NoteHeader bnh in bnhl)
            {
                // get content for base note
                //NoteHeader nc = await NoteDataManager.GetNoteById(_db, bnh.Id);

                NoteHeader nc = null;
                req = bnh.Id.ToString();

                // get nc from Export2

                // format it and write it
                await WriteNote(sw, nc, bnh, isHtml, false, ProdUri);

                // get ordered list of responses
                //List<NoteHeader> rcl = await NoteDataManager.GetOrderedListOfResponses(_db, nfid, bnh);

                req = "" + nfid + "." + model.ArchiveNumber + "." + model.NoteOrdinal;
                List<NoteHeader> rcl = null;

                // get rcl from Export3


                await sw.WriteLineAsync();
                // extra stuff for collapsable responses
                if (isCollapsible && isHtml && rcl.Any())
                {
                    await sw.WriteLineAsync("<div class=\"container\"><div class=\"panel-group\">" +
                        "<div class=\"panel panel-default\"><div class=\"panel-heading\"><div class=\"panel-title\"><a data-toggle=\"collapse\" href=\"#collapse" +
                        nc.NoteOrdinal + "\">Toggle " + bnh.ResponseCount + " Response" + (bnh.ResponseCount > 1 ? "s" : "") + "</a></div></div><div id = \"collapse" + nc.NoteOrdinal +
                        "\" class=\"panel-collapse collapse\"><div class=\"panel-body\">");
                }
                // loop over each respponse for a base note
                foreach (NoteHeader rc in rcl)
                {
                    await WriteNote(sw, rc, bnh, isHtml, true, ProdUri);
                }
                // extra stuff to terminate collapsable responses
                if (isCollapsible && isHtml && rcl.Any())
                {
                    await sw.WriteLineAsync("</div></div></div></div></div> ");
                }
            }

            if (isHtml)  // end the html
            {
                await sw.WriteLineAsync("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.3/jquery.min.js\" ></script >");
                await sw.WriteLineAsync("<script src = \"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/js/bootstrap.min.js\" ></script >");
                await sw.WriteLineAsync("<script src = \"https://notes2020.drsinder.com/js/prism.min.js\" ></script >");

                await sw.WriteLineAsync("</body></html>");
            }

            // make sure all output is written to stream and rewind it
            await sw.FlushAsync();
            ms.Seek(0, SeekOrigin.Begin);
            // send stream to caller
            return ms;
        }


        /// <summary>
        /// Does the creation of a note item as html fragment
        /// </summary>
        /// <param name="sw">Memory stream writer</param>
        /// <param name="nc">Note content object</param>
        /// <param name="bnh">Base note header object</param>
        /// <param name="isHtml">html vs plain text</param>
        /// <param name="isResponse">response or base note</param>
        /// <returns></returns>
        public static async Task<bool> WriteNote(StreamWriter sw, NoteHeader nc, NoteHeader bnh, bool isHtml, bool isResponse, string ProdUri)
        {
            StringBuilder sb;
            if (isHtml)
            {
                string extra = "";
                if (nc.NoteContent.NoteBody.StartsWith("<DIV STYLE="))
                    extra = "-client";

                if (!isResponse)
                    await sw.WriteLineAsync("<div class=\"base-note" + extra + "\">");
                else
                    await sw.WriteLineAsync("<div class=\"response-note" + extra + "\">");

                await sw.WriteLineAsync("<h3>");
            }

            if (!isResponse)
            {
                // write base note header
                await sw.WriteLineAsync("Note: " + nc.NoteOrdinal + " - Subject: "
                + nc.NoteSubject + (isHtml ? "<br />" : " - ") + "Author: " + nc.AuthorName + " - "
                + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " - "
                + bnh.ResponseCount + " Response" + (bnh.ResponseCount > 1 || bnh.ResponseCount == 0 ? "s" : ""));
            }
            else
            {
                // write response note header
                await sw.WriteLineAsync("Note: " + nc.NoteOrdinal + " - Subject: "
                + nc.NoteSubject + (isHtml ? "<br />" : " - ") + "Author: " + nc.AuthorName + " - "
                + nc.LastEdited.ToShortDateString() + " " + nc.LastEdited.ToShortTimeString() + " - "
                + "Response " + nc.ResponseOrdinal + " of " + bnh.ResponseCount);
                await sw.WriteLineAsync((isHtml ? "<br />" : string.Empty) + "Base Note Subject: " + bnh.NoteSubject);
            }

            if (isHtml)
            {
                await sw.WriteLineAsync("</h3>");
            }

            // Do tags

            if (!isHtml || (nc.Tags != null && nc.Tags.Count > 0))
            {
                await sw.WriteAsync((isHtml ? "<h5>" : "") + "Tags - ");
                foreach (Tags item in nc.Tags)
                {
                    await sw.WriteAsync(item.Tag + " ");
                }
                await sw.WriteLineAsync((isHtml ? "</h5>" : ""));
            }

            if (!isHtml || !string.IsNullOrEmpty(nc.NoteContent.DirectorMessage))
            {
                await sw.WriteLineAsync((isHtml ? "<h5>" : "") + "Director Message - " + nc.NoteContent.DirectorMessage + (isHtml ? "</h5>" : ""));
            }
            await sw.WriteLineAsync();
            if (isHtml)
            {
                sb = new StringBuilder();
                sb.Append("<h4>");
                sb.Append("<a href=\"");
                sb.Append(ProdUri + "/NoteDisplay/CreateResponse/" + nc.BaseNoteId +
                          "\" target=\"_blank\">New Response</a>");
                sb.Append("</h4>");
                await sw.WriteLineAsync(sb.ToString());
            }

            if (isHtml)
                await sw.WriteLineAsync(nc.NoteContent.NoteBody.Replace("<br />", "<br />\r\n") + "</div>");
            else
                await sw.WriteLineAsync(nc.NoteContent.NoteBody.Replace("<br />", "\r\n"));
            await sw.WriteLineAsync();

            return true;
        }


    }
}
