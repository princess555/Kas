using CTS_Models.DBContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.SessionState;
using System.Web.UI;

namespace CTS_Analytics.Filters
{

    public class CtsAuthAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public CtsAuthAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string user = httpContext.User.Identity.Name;
            user = user.Substring(7);

            var db = new CtsDbContext();

            bool isValid = db.CtsUserCtsRole.Any(d => d.CtsRole_RoleName == _allowedRoles.ToString());

            if (isValid)
            {
                return true;

            }
            else
            {
                return false;
            }
        }
    }
}