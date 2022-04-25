using InClassVoting.Filter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InClassVoting.Areas.teacher.Controllers
{
    [AccessAuthenticationFilter]
    [UserAuthorizeFilter("Teacher")]
    public class HomeController : Controller
    {
        [HandleError]
        public ActionResult Home()
        {
            
            ViewBag.UserName = Convert.ToString(HttpContext.Session["Name"]);
            ViewBag.ImageURL = Convert.ToString(HttpContext.Session["ImageURL"]);
            return View();
        }
    }
}