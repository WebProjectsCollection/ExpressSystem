using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniformTypeController : ControllerBase
    {
        // GET: api/UniformType
        [HttpGet]
        public MyResult GetList(int siteId)
        {
            List<UniformType> list = UniformTypeBLL.getAllBySite(siteId);
            return MyResult.OK(list);
        }
        [HttpGet("session")]
        public MyResult GetSessions(int siteId)
        {
            List<string> list = UniformTypeBLL.getAllSessions(siteId);
            return MyResult.OK(list);
        }

        // POST: api/UniformType
        [HttpPost]
        public MyResult Post([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            int siteId = Convert.ToInt32(obj["siteId"]);
            JArray types = JArray.FromObject(obj["types"]);
            List<UniformType> list = types.ToObject<List<UniformType>>();

            bool re = UniformTypeBLL.SaveData(siteId, list);
            return re ? MyResult.OK() : MyResult.Error();
        }
    }
}
