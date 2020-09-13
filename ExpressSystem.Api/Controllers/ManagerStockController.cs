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
    public class ManagerStockController : ControllerBase
    {
        // GET: api/ManagerStock
        [HttpGet]
        public MyResult Get([FromQuery]RecordSearchParam param)
        {
            List<Object> uniforms = UniformMgBLL.ManagerStockList(param);
            return MyResult.OK(uniforms);
        }

        [HttpPost("transfer")]
        public MyResult Transfer([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            int siteId = Convert.ToInt32(obj["siteId"]);
            long fromEmployeeId = Convert.ToInt64(obj["fromEmployeeId"]);
            long toEmployeeId = Convert.ToInt64(obj["toEmployeeId"]);
            JArray uniforms = JArray.FromObject(obj["uniforms"]);
            List<UniformType> uniformList = uniforms.ToObject<List<UniformType>>();

            UniformMgBLL.Transfer(siteId, fromEmployeeId, toEmployeeId, uniformList);
            return MyResult.OK();
        }
    }
}
