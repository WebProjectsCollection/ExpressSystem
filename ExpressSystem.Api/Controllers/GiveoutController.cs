using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiveoutController : ControllerBase
    {
        [HttpPost]
        public MyResult Post([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            int siteId = Convert.ToInt32(obj["siteId"]);
            long fromEmployeeId = Convert.ToInt64(obj["fromEmployeeId"]);
            long toEmployeeId = Convert.ToInt64(obj["toEmployeeId"]);
            int type = Convert.ToInt32(obj["applyType"]);
            int number = Convert.ToInt32(obj["applyNumber"]);
            JArray uniforms = JArray.FromObject(obj["uniforms"]);
            List<UniformType> uniformList = uniforms.ToObject<List<UniformType>>();

            UniformMgBLL.Giveout(siteId, type, fromEmployeeId, toEmployeeId, number, uniformList);
            return MyResult.OK();
        }

        [HttpGet("record")]
        public MyResult Record([FromQuery]RecordSearchParam param)
        {
            int total;
            List<Object> data = UniformMgBLL.OutAndInRecord(param, out total);

            var result = new
            {
                total = total,
                list = data
            };
            return MyResult.OK(result);
        }

        [HttpGet("export")]
        public IActionResult Export([FromQuery]RecordSearchParam param)
        {

            int total;
            param.PageSize = 999999;
            List<Object> list = UniformMgBLL.OutAndInRecord(param, out total);

            Dictionary<string, string> fields = new Dictionary<string, string>();
            fields.Add("ChineseName", "姓名");
            fields.Add("EmployeeID", "工号");
            fields.Add("Department", "部门");
            fields.Add("Costcenter", "成本中心");
            fields.Add("UniformStyle", "领取的工服类型");
            fields.Add("Number", "领取数量");
            fields.Add("FromEmployee", "发放人");
            fields.Add("OutTime", "领取时间");
            fields.Add("BackNumber", "退还数量");
            fields.Add("BackTime", "退还时间");

            //使用
            ExcelHelper _export = new ExcelHelper(fields);
            byte[] bytes = _export.Export<Object>(list);

            return File(bytes, "application/vnd.ms-excel", "领取退还工服记录.xls");
        }
    }
}
