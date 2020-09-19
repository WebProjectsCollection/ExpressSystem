using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.WeChartApi.Entity
{
    public enum OrderStatusEnum
    {
        /// <summary>
        /// 已下单/待发货
        /// </summary>
        Created = 1001,
        /// <summary>
        /// 已发货
        /// </summary>
        HasSend = 1011,
        /// <summary>
        /// 飞机运输中
        /// </summary>
        InFlight = 1012,
        /// <summary>
        /// 待派送
        /// </summary>
        WaitDelivery = 1013,
        /// <summary>
        /// 派送中
        /// </summary>
        Deliverying = 1014,
        /// <summary>
        /// 已签收
        /// </summary>
        BeenSigned = 1021,
        /// <summary>
        /// 已丢失
        /// </summary>
        Lost = 1031,
    }
    public class OrderStatus
    {
        public static string GetStatus(string status)
        {
            switch (status)
            {
                case "1001": return "已下单";
                case "1011": return "已发货";
                case "1012": return "飞机运输中";
                case "1013": return "到津待派送";
                case "1014": return "派送中";
                case "1021": return "已签收";
                case "1031": return "已丢失";
                default: return "--";
            }
        }
    }
}
