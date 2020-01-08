using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using FilterAttribute = System.Web.Http.Filters.FilterAttribute;
using IExceptionFilter = System.Web.Mvc.IExceptionFilter;

namespace CTS_Analytics.ErrorHandler
{
    public class IndexException : FilterAttribute, IExceptionFilter
    {
       
        public void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled && filterContext.Exception is IndexOutOfRangeException)
            {
                filterContext.Result = new RedirectResult("/Content/ExceptionFound.html");
                filterContext.ExceptionHandled = true;
            }
        }
    }
}