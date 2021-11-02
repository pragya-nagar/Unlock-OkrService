using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OKRService.Service.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace OKRService.WebCore.Filters
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionFiltersAttribute : ExceptionFilterAttribute
    {
        private readonly ICommonService commonService;

        public ExceptionFiltersAttribute(ICommonService commonServices) : base()
        {
            commonService = commonServices;
        }

        public override void OnException(ExceptionContext context)
        {
            var controller = string.Empty;
            var action = string.Empty;

            var statusCode = HttpStatusCode.InternalServerError;

            if (context.Exception is DataNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)statusCode;

            if (context.RouteData != null)
            {
                action = context.RouteData.Values["action"].ToString();
                controller = context.RouteData.Values["controller"].ToString();
            }

            context.Result = new JsonResult(new
            {
                error = new[] { context.Exception.Message },
                stackTrace = context.Exception.StackTrace
            });

            commonService.SaveLog(controller, action, context.Exception.ToString() + "InnerException" + context.Exception.InnerException);

        }
    }
}
