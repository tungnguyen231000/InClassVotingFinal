using InClassVoting.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InClassVoting.Areas.Admin.Controllers
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Admin")]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Home()
        {
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            return View();
        }
    }
}