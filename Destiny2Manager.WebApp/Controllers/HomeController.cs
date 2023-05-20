using Destiny2Manager.Models.Bungie.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Destiny2Manager_WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
			if (BungieConstants.Auth != null)
			{
				return RedirectToAction("Index", "Destiny");
			}
			return View();
        }
        public IActionResult About() {
            if (BungieConstants.Auth != null)
            {
                return RedirectToAction("Index", "Home");
            }
			return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
			//if (BungieConstants.Auth != null)
			//{
			//	return RedirectToAction("Index", "Home");
			//}
			return View();
        }
    }
}
