using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        // GET: api/Home
        [HttpGet]
        public MyResult Get(int siteId)
        {
            DateTime now = DateTime.Now;
            DateTime startTime = now.AddDays(1 - now.Day).AddMonths(-1);
            DateTime endTime = now.AddDays(1 - now.Day).AddDays(-1);
            List<Object> uniforms = HomeBLL.GetUniformSummary(siteId, startTime, endTime);
            List<Object> employees = HomeBLL.GetEmployeeSummary(siteId);
            return MyResult.OK(new { startTime, endTime, uniforms, employees });
        }
    }
}
