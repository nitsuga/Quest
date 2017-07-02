using System.Web.Mvc;

namespace Quest.Mobile.Controllers
{
    public class DefaultController : Controller
    {
        //
        // GET: /Default/
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
	}
}