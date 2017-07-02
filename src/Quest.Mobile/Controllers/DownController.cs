using System.Web.Mvc;

namespace Quest.Mobile.Controllers
{
    public class DownController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewBag.Message = MvcApplication.FailureMessage;
            return View();
        }
    }
}
