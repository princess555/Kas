using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace CTS_Core
{
	public class CtsAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{

			return CtsAuthorizeProvider.CheckIsInRole(httpContext.User.Identity, base.Roles) || base.AuthorizeCore(httpContext);
		}
	}
}
