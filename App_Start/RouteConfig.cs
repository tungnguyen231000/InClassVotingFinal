using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace InClassVoting
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "Default",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "HomeTotal", action = "Login", id = UrlParameter.Optional }
               );

            routes.MapRoute(
                name: "Teacher",
                url: "Teacher/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Home", id = UrlParameter.Optional }
            ).DataTokens.Add("area", "Teacher");

            routes.MapRoute(
                name: "Student",
                url: "Student/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Home", id = UrlParameter.Optional }
            ).DataTokens.Add("area", "Student");
        }
    }
}
