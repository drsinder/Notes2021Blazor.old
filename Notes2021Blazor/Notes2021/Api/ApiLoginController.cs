using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;

using System;
using System.Text;
using System.Threading.Tasks;


namespace Notes2021.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiLoginController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly NotesDbContext _context;


        public ApiLoginController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, NotesDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }


        /// <summary>
        /// Login Api (Post)
        /// </summary>
        /// <returns>string of three comma delimited fields.
        /// 0: user email, 1: userId,  2: User unique login token
        /// This entire string is sent as a header named "authentication"
        /// for all future requests.  It is used to
        /// validate the user.  User email (login) and password are
        /// included as the body separated by a "/".
        /// </returns>
        // POST: api/
        [HttpPost]
        public async Task<string> Login()
        {
            long? lth = Request.ContentLength;

            var str = Request.Body;
            byte[] by = new byte[200];
            if (lth != null)
            {
                try
                {
                    await str.ReadAsync(by, 0, (int)lth);
                }
                catch (Exception ex)
                {
                    string x = ex.Message;
                }
                string converted = Encoding.UTF8.GetString(by, 0, (int)lth);

                string[] items = converted.Split('/');
                var result = await _signInManager.PasswordSignInAsync(items[0], items[1], false, true);

                if (result.Succeeded)
                {
                    IdentityUser me = await _userManager.FindByEmailAsync(items[0]);

                    UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);

                    if (string.IsNullOrEmpty(appMe.MyGuid))
                    {
                        string myGuid = Guid.NewGuid().ToString();
                        appMe.MyGuid = myGuid;
                        await _userManager.UpdateAsync(me);
                    }

                    string mykey = items[0] + "," + me.Id + "," + appMe.MyGuid;

                    return mykey;
                }
            }

            return null;    // tell client login failed
        }


        /// <summary>
        /// GET
        /// Get a user access object NoteAccess for a file and current user.
        /// </summary>
        /// <param name="id">file Id</param>
        /// <returns>NoteAccess object for user in file</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<NoteAccess>> GetAccess(int id)
        {
            string authHeader = Request.Headers["authentication"];
            string[] auths = authHeader.Split(',');

            IdentityUser me = await _userManager.FindByIdAsync(auths[1]);
            UserData appMe = await _context.UserData.SingleAsync(p => p.UserId == me.Id);

            if (String.Compare(auths[2], appMe.MyGuid, StringComparison.Ordinal) != 0)
                return new NoteAccess();
            string userID = auths[1];
            NoteAccess myAcc = await AccessManager.GetAccess(_context, userID, id, 0);
            return myAcc;
        }
    }
}