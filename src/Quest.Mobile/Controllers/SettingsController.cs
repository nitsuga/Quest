using System.Web;
using System.Web.Mvc;
#if OAUTH
using Microsoft.Owin.Security;
#endif

namespace Quest.Mobile.Controllers
{
#if OAUTH
    [Authorize]
#endif
    public class SettingsController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.Message = "Quest Mobile - Settings";
#if OAUTH
            ViewBag.IsAdmin = AuthenticationManager.User.IsInRole("administrator");
#else
            ViewBag.IsAdmin = true;
#endif
            return View();
        }

#if OAUTH
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
#endif
    }
}
