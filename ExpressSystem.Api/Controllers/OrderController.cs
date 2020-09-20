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

        [HttpPost("update")]
        public MyResult Update([FromBody] OrderInfo data)
        {
            bool re = OrderBLL.UpdateOrder(data);
            return re ? MyResult.OK() : MyResult.Error();
        }
    }
}
