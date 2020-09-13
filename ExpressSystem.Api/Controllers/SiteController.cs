using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        // GET: api/Site
        [HttpGet]
        public MyResult Get()
        {
            List<Object> siteList = SiteBLL.GetSiteList();
            return MyResult.OK(siteList);
        }

        // GET: api/Site/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Site
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Site/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
