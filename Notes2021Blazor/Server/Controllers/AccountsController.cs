using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes2021Blazor.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private static UserModel LoggedOutUser = new UserModel { IsAuthenticated = false };

        private readonly UserManager<IdentityUser> _userManager;
        private readonly NotesDbContext _db;

        public AccountsController(UserManager<IdentityUser> userManager, NotesDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RegisterModel model)
        {
            var newUser = new IdentityUser { UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);

                return Ok(new RegisterResult { Successful = false, Errors = errors });
            }

            IdentityUser me = await _userManager.FindByEmailAsync(model.Email);
            UserData userData = new UserData
            {
                UserId = me.Id,
                DisplayName = model.DisplayName,
                TimeZoneID = Globals.TimeZoneDefaultID,
                MyGuid = Guid.NewGuid().ToString()
            };
            _db.UserData.Add(userData);

            // Add all new users to the User role
            await _userManager.AddToRoleAsync(newUser, "User");

            if (newUser.Email == Globals.PrimeAdminEmail)
                await _userManager.AddToRoleAsync(newUser, "Admin");


            await _db.SaveChangesAsync();

            return Ok(new RegisterResult { Successful = true });
        }
    }

    public class UserModel
    {
        public bool IsAuthenticated { get; set; }
        public UserData userData { get; set; }
    }

}