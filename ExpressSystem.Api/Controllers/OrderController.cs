using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public class OrderController : ControllerBase
    {
        // GET: api/User
        [HttpGet]
        public MyResult GetList([FromQuery]OrderInfoParam param)
        {
            int total;
            List<OrderInfo> roleList = OrderBLL.GetOrderList(param, out total);

            var result = new
            {
                total,
                list = roleList
            };
            return MyResult.OK(result);
        }

        [HttpGet("detail")]
        public MyResult GetDetail([FromQuery]OrderInfoParam param)
        {
            OrderInfo orderInfo = OrderBLL.GetOrderDetail(param);
            return MyResult.OK(orderInfo);
        }

        [HttpGet("batchNos")]
        public MyResult GetBatchNos()
        {
            List<Object> result = OrderBLL.GetBatchNos();
            return MyResult.OK(result);
        }

        // POST: api/User
        [HttpPost]
        public MyResult Post([FromBody] OrderInfo data)
        {

            if (string.IsNullOrWhiteSpace(data.OrderNumber))
            {
                return MyResult.Error("快递单号不能为空！");
            }
            if (string.IsNullOrWhiteSpace(data.JBBWAddress))
            {
                return MyResult.Error("津巴布韦地址不能为空！");
            }

            bool re = OrderBLL.AddNewOrder(data);
            return re ? MyResult.OK() : MyResult.Error();
        }

        [HttpPost("update")]
        public MyResult Update([FromBody] OrderInfo data)
        {
            bool re = OrderBLL.UpdateOrder(data);
            return re ? MyResult.OK() : MyResult.Error();
        }

        [HttpPost("updatestatus")]
        public MyResult BatchUpdateStatus([FromBody] OrderStatusParam param)
        {
            bool re = OrderBLL.BatchUpdateStatus(param);
            return re ? MyResult.OK() : MyResult.Error();
        }

        [HttpPost("updateStatusByBatchNumber")]
        public MyResult UpdateStatusByBatchNumber([FromBody] BatchUpdateParam param)
        {
            bool re = OrderBLL.UpdateStatusByBatchNumber(param);
            return re ? MyResult.OK() : MyResult.Error();
        }
        [Produces("application/json")]
        [Consumes("application/json", "multipart/form-data")]
        [HttpPost("importOrder")]
        public IActionResult importOrder(IFormFile file, [FromQuery]string userName)
        {
            try
            {
                DataTable dt = ExcelHelper.ReadExcelToDataTable(file.OpenReadStream(), "Sheet1");

                string sql = @"INSERT INTO tmp_importorder
                                (`BatchID`,
                                `ORDER_NUM`,
                                `SENDER_PHONE`,
                                `SENDER_NAME`,
                                `SENDER_ADDRESS`,
                                `JBBW_PHONE`,
                                `JBBW_NAME`,
                                `JBBW_ADDRESS`,
                                `BATCH_NUMBER`,
                                `CreatedBy`) VALUES ";
                List<string> values = new List<string>();
                string batchId = Guid.NewGuid().ToString("N");
                string batchNumber = DateTime.Now.ToString("第yyyyMMdd批");
                foreach (DataRow row in dt.Rows)
                {
                    string orderNum = Convert.ToString(row["快递单号"]);
                    string jbbwPhone = Convert.ToString(row["津巴布韦电话"]);
                    string jbbwAddress = Convert.ToString(row["津巴布韦地址"]);
                    string jbbwName = Convert.ToString(row["津巴布韦姓名"]);
                    string senderName = Convert.ToString(row["寄件人姓名"]);
                    string senderPhone = Convert.ToString(row["寄件人电话"]);
                    string senderAddress = Convert.ToString(row["寄件人地址"]);

                    values.Add($"('{batchId}','{orderNum}','{senderPhone}','{senderName}','{senderAddress}','{jbbwPhone}','{jbbwName}','{jbbwAddress}','{batchNumber}','{userName}')");
                }
                if (values.Count == 0)
                {
                    throw new MsgException("导入人员信息为空！");
                }
                JabMySqlHelper.ExecuteNonQuery(Config.DBConnection, sql + string.Join(",", values));

                DataTable resultdt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, $"call Sp_confirmImportOrder('{batchId}')");
                List<OrderInfo> recordList = new List<OrderInfo>();
                if (resultdt != null && resultdt.Rows.Count > 0)
                {
                    foreach (DataRow row in resultdt.Rows)
                    {
                        recordList.Add(new OrderInfo()
                        {
                            OrderNumber = Converter.TryToString(row["ORDER_NUM"]),
                            JBBWPhone = Converter.TryToString(row["JBBW_PHONE"]),
                            JBBWName = Converter.TryToString(row["JBBW_NAME"]),
                            JBBWAddress = Converter.TryToString(row["JBBW_ADDRESS"]),
                            SenderName = Converter.TryToString(row["SENDER_NAME"]),
                            SenderPhone = Converter.TryToString(row["SENDER_PHONE"]),
                            SenderAddress = Converter.TryToString(row["SENDER_ADDRESS"]),
                            Status = Converter.TryToString(row["Status"]),
                            BatchNo = Converter.TryToString(row["BATCH_NUMBER"]),
                        });
                    }
                }
                JabMySqlHelper.ExecuteNonQuery(Config.DBConnection, $"delete from tmp_importorder where BatchID = '{batchId}'");
                return Ok(new
                {
                    status = 200,
                    msg = $"导入完成，今日包裹批次号：{batchNumber}",
                    data = recordList
                });
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    status = 500,
                    msg = "导入失败:" + e.Message,
                    error = "导入失败:" + e.Message + Environment.NewLine + e.StackTrace
                });
            }
        }
    }
}
