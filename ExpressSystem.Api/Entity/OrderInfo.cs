using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.Api.Entity
{
    public class OrderInfo
    {
        public long ID { get; set; }
        public string UserName { get; set; }

        public string OrderNumber { get; set; }

        public string JBBWName { get; set; }
        public string JBBWPhone { get; set; }
        public string JBBWAddress { get; set; }
        public string SenderName { get; set; }
        public string SenderPhone { get; set; }
        public string SenderAddress { get; set; }
        public string Remarks { get; set; }
        public string Weight { get; set; }
        public string BatchNo { get; set; }
        public string FlightNumber { get; set; }
        public string LandingTime { get; set; }
        public string Status { get; set; }
        public string StatusStr { get; set; }
        public string CreateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdateTime { get; set; }
        public string UpdatedBy { get; set; }

    }

    public class OrderInfoParam
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public long? ID { get; set; }
        public string OrderNumber { get; set; }
        public string KeyWord { get; set; }
        public string FlightNumber { get; set; }
        public string CreateTimeStartStr { get; set; }
        public string CreateTimeEndStr { get; set; }
        public string Status { get; set; }
    }

    public class OrderStatusParam
    {
        public List<OrderInfo_F> dicOrders { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }

        public class OrderInfo_F
        {
            public string Id { get; set; }
            public string Order_Num { get; set; }
        }
    }
}
