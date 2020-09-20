using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpressSystem.WeChartApi.Entity;

namespace ExpressSystem.WeChartApi.Filters
{
    public class GlobalExceptions : IExceptionFilter

    {

        public GlobalExceptions()
        {

        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is MsgException)
            {
                // 返回ERROR响应码
                context.Result = new JsonResult(MyResult.Error(context.Exception.Message));
                context.ExceptionHandled = true;
                context.HttpContext.Response.Clear();
                context.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
                context.HttpContext.Response.StatusCode = 200;
            }
        }

    }
}
