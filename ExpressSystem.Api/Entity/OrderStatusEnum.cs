using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.Api.Entity
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
    public static class OrderStatus
    {
        public static string GetStatus(string status)
        {
            switch (status)
            {
                case "1001": return "已下单";
                case "1011": return "已揽件";
                case "1012": return "已发货/运送中";
                case "1013": return "到津待派送";
                case "1014": return "派送中";
                case "1021": return "已签收";
                case "1031": return "已丢失";
                default: return "--";
            }
        }
    }

    public static class OrderStatusDetaill
    {
        public static string GetStatus(string status)
        {
            switch (status)
            {
                case "1001": return "您的订单已录入，正在等待揽件";
                case "1011": return "您的订单已被揽件，正在极速送往机场的路上";
                case "1012": return "您的订单正在运输中，预计两个小时到津";
                case "1013": return "您的订单已到津，正在等待派送";
                case "1014": return "您的订单正在派送中，请保持电话通畅";
                case "1021": return "您的订单已签收，感谢您对飞箭国际快递的关注";
                default: return "--";
            }
        }
    }
}
