/*--------------------------------------------------------------------------
**
**  Copyright (c) 2019, Dale Sinder
**
**  Name: FileController.cs
**
**  Description:
**      File Controller for Notes 2020
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;
using System.Threading.Tasks;

namespace Notes2021.Controllers
{
    public class File : Controller
    {
        //private NotesDbContext _context;
        private readonly NotesDbContext _sqlcontext;

        public File(NotesDbContext context, NotesDbContext sqlcontext)
        {
            //_context = context;
            _sqlcontext = sqlcontext;
        }

        public async Task<FileResult> GetById(long? id)
        {
            if (id == null)
                return FileNotFound();

            try
            {
                SQLFile sQlFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileId == id);
                if (sQlFile == null)
                    return FileNotFound();

                return File((await (_sqlcontext.SQLFileContent.SingleAsync(m => m.SQLFileId == id))).Content,
                    sQlFile.ContentType,
                    sQlFile.FileName);
            }
            catch
            { return FileNotFound(); }
        }

        public async Task<FileResult> GetByName(string id)
        {
            if (id == null)
                return FileNotFound();

            try
            {
                SQLFile sQlFile = await _sqlcontext.SQLFile.SingleAsync(m => m.FileName == id);
                if (sQlFile == null)
                    return FileNotFound();

                return File((await (_sqlcontext.SQLFileContent.SingleAsync(m => m.SQLFileId == sQlFile.FileId))).Content,
                    sQlFile.ContentType,
                    sQlFile.FileName);
            }
            catch
            { return FileNotFound(); }
        }

        private FileResult FileNotFound()
        {
            return File("~/images/NotFound.png", "image/png");
        }
    }
}
