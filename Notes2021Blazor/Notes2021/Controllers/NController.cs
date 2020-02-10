using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes2021Blazor.Shared;


namespace Notes2021.Controllers
{
    public class NController : Controller
    {
        protected readonly UserManager<IdentityUser> _userManager;
        protected readonly SignInManager<IdentityUser> _signInManager;
        protected readonly NotesDbContext _db;

        public NController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            NotesDbContext NotesDbContext
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = NotesDbContext;
        }

        public override ViewResult View()
        {
            SetStuff();

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View();
        }

        public override ViewResult View(object model)
        {
            SetStuff();

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View(model);
        }

        public override ViewResult View(string viewName)
        {
            SetStuff();

            return base.View(viewName);
        }

        public override ViewResult View(string viewName, object model)
        {
            SetStuff();

            return base.View(viewName, model);
        }

        ////

        public ViewResult View(UserManager<IdentityUser> userManager)
        {
            SetStuff(userManager);

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View();
        }

        public ViewResult View(UserManager<IdentityUser> userManager, object model)
        {
            SetStuff(userManager);

            // ReSharper disable once Mvc.ViewNotResolved
            return base.View(model);
        }

        public ViewResult View(UserManager<IdentityUser> userManager, string viewName)
        {
            SetStuff(userManager);

            return base.View(viewName);
        }

        public ViewResult View(UserManager<IdentityUser> userManager, string viewName, object model)
        {
            SetStuff(userManager);

            return base.View(viewName, model);
        }


        ////


        private void SetStuff()
        {
            ViewBag.UserId = "";
            ViewBag.UserDisplayName = " ";
            if (_signInManager.IsSignedIn(User))
            {
                ViewBag.UserDisplayName = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db);
                ViewBag.UserId = _userManager.GetUserId(User);
                ViewBag.DisplayName = NoteDataManager.GetUserDisplayName(_userManager, User, _db);
            }

            string b2 = HttpContext.Session.GetString("MyStyle");
            if (!string.IsNullOrEmpty(b2))
            {
                ViewBag.MyStyle = b2;
            }

            ViewBag.PusherKey = Globals.PusherKey;
            ViewBag.PusherCluster = Globals.PusherCluster;
            ViewBag.PathBase = Globals.PathBase;
        }


        private async void SetStuff(UserManager<IdentityUser> userManager)
        {
            ViewBag.PathBase = Globals.PathBase;
            ViewBag.PusherKey = Globals.PusherKey;
            ViewBag.PusherCluster = Globals.PusherCluster;
            ViewBag.UserId = "";
            ViewBag.UserDisplayName = " ";
            if (_signInManager.IsSignedIn(User))
            {
                ViewBag.UserId = _userManager.GetUserId(User);
                ViewBag.UserDisplayName = NoteDataManager.GetSafeUserDisplayName(_userManager, User, _db);
            }

            string b2 = HttpContext.Session.GetString("MyStyle");
            if (!string.IsNullOrEmpty(b2))
            {
                ViewBag.MyStyle = b2;
            }
            else
            {
                if (_signInManager.IsSignedIn(User))
                {
                    try
                    {
                        IdentityUser user = await userManager.Users.SingleAsync(p => p.Id == userManager.GetUserId(User));
                    }
                    catch
                    {
                        goto gone;
                    }
                    UserData appUser;
                    string userid = _userManager.GetUserId(User);
                    appUser = await _db.UserData.SingleOrDefaultAsync(p => p.UserId == userid);

                    if (!string.IsNullOrEmpty(appUser.MyStyle))
                    {
                        ViewBag.DisplayName = appUser.DisplayName;
                        HttpContext.Session.SetString("MyStyle", appUser.MyStyle);
                    }
                    else
                    {
                        ViewBag.DisplayName = " ";
                        HttpContext.Session.SetString("MyStyle", " ");
                    }
                }
            }
        gone:;
        }

    }
}