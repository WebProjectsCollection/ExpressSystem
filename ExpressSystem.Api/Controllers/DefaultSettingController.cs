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
    public class DefaultSettingController : ControllerBase
    {

        // GET: api/DefaultSetting
        [HttpGet]
        public MyResult Get(int siteId)
        {
            object data = DefaultSettingBLL.GetData(siteId);
            return MyResult.OK(data);
        }

        // POST: api/DefaultSetting
        [HttpPost]
        public MyResult Post([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            int siteId = Convert.ToInt32(obj["siteId"]);
            string uniformType = Convert.ToString(obj["uniformType"]);
            int applyNumber = Convert.ToInt32(obj["applyNumber"]);
            string hrEmails = Convert.ToString(obj["hrEmails"]);

            bool re = DefaultSettingBLL.SaveData(siteId, uniformType, applyNumber, hrEmails);
            return re ? MyResult.OK() : MyResult.Error();

        }
    }
}
