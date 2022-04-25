using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace InClassVoting.Filter
{
    public class UserAuthorizeFilter : AuthorizeAttribute
    {
        private readonly string[] allowedroles;
        public UserAuthorizeFilter(params string[] roles)
        {
            this.allowedroles = roles;
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;
            string userRole = Convert.ToString(httpContext.Session["User"]);
            if (!string.IsNullOrEmpty(userRole))
                foreach (var role in allowedroles)
                {
                    if (role == userRole) return true;
                }
            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string httpContext = "~/Error/UnAuthorized";
            filterContext.Result = new RedirectResult(httpContext);
        }
    }
}