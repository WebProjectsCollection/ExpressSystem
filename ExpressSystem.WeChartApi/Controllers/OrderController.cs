using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpressSystem.WeChartApi.BLL;
using ExpressSystem.WeChartApi.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpressSystem.WeChartApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpPost("AddNewOrder")]
        public MyResult AddNewOrder([FromBody] OrderInfo data)
        {
            if (string.IsNullOrWhiteSpace(data.OrderNumber))
            {
                return MyResult.Error("快递单号不能为空！");
            }
            if (string.IsNullOrWhiteSpace(data.SenderPhone))
            {
                return MyResult.Error("寄件人电话不能为空！");
            }
            if (string.IsNullOrWhiteSpace(data.JBBWName))
            {
                return MyResult.Error("津巴布韦姓名不能为空！");
            }
            if (string.IsNullOrWhiteSpace(data.JBBWPhone))
            {
                return MyResult.Error("津巴布韦电话不能为空！");
            }
            if (string.IsNullOrWhiteSpace(data.JBBWAddress))
            {
                return MyResult.Error("津巴布韦地址不能为空！");
            }
            bool re = OrderBLL.AddNewOrder(data);
            return re ? MyResult.OK() : MyResult.Error();   
        }

        [HttpGet("GetOrderInfo")]
        public MyResult GetOrderInfo(string order_num)
        {
            List<StatusInfo> statusInfo = OrderBLL.GetOrderInfo(order_num);
            var result = new
            {
                list = statusInfo
            };
            return MyResult.OK(result);
        }
    }
}