using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendbackController : ControllerBase
    {
        // GET: api/Sendback
        [HttpPost]
        public MyResult Post([FromBody] List<SendbackRecord> list)
        {
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    UniformMgBLL.Sendback(item.giveoutRecordId, item.backNumber);
                }
            }
            return MyResult.OK();
        }

        [HttpGet("record")]
        public MyResult Record([FromQuery]RecordSearchParam param)
        {
            int total;
            List<DebtRecord> data = UniformMgBLL.ChargeRecord(param, out total);

            var result = new
            {
                total,
                list = data
            };
            return MyResult.OK(result);
        }

        [HttpGet("export")]
        public IActionResult Export([FromQuery]RecordSearchParam param)
        {

            int total;
            param.PageSize = 999999;
            List<DebtRecord> list = UniformMgBLL.ChargeRecord(param, out total);

            Dictionary<string, string> fields = new Dictionary<string, string>();
            fields.Add("ChineseName", "姓名");
            fields.Add("EmployeeID", "员工工号");
            fields.Add("Department", "部门");
            fields.Add("InDate", "入职时间");
            fields.Add("LeaveDate", "离职时间");
            fields.Add("Costcenter", "成本中心");
            fields.Add("Debt", "应扣款");
            fields.Add("Details", "明细");

            //使用
            ExcelHelper _export = new ExcelHelper(fields);
            byte[] bytes = _export.Export<DebtRecord>(list);

            return File(bytes, "application/vnd.ms-excel", "扣款报表.xls");
        }
    }
}
