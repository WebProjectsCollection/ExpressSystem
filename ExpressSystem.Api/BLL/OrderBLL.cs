using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ExpressSystem.Api.Entity;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.BLL
{
    public class OrderBLL
    {
        public static bool AddNewOrder(OrderInfo data)
        {
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection,
                              "select count(*) from ex_orderinfo where ORDER_NUM=@OrderNumber;",
                          new MySqlParameter("@OrderNumber", data.OrderNumber));
            if (Converter.TryToInt32(re) > 0)
            {
                throw new MsgException("快递单号已存在，请检查！");
            }

            bool insertStuatus = JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    $"INSERT INTO ex_orderinfo (ORDER_NUM,JBBW_NAME,JBBW_PHONE,JBBW_ADDRESS,SENDER_NAME,SENDER_PHONE,SENDER_ADDRESS,STATUS,REMARKS,WEIGHT,BATCH_NUMBER,CreatedBy) " +
                    $"VALUES (@OrderNumber,@JBBWName,@JBBWPhone,@JBBWAddress,@SenderName,@SenderPhone,@SenderAddress,@Status,@Remark,@Weight,@BatchNo,@UserName);",
                new MySqlParameter("@OrderNumber", data.OrderNumber),
                new MySqlParameter("@JBBWName", data.JBBWName),
                new MySqlParameter("@JBBWPhone", data.JBBWPhone),
                new MySqlParameter("@JBBWAddress", data.JBBWAddress),
                new MySqlParameter("@SenderName", data.SenderName),
                new MySqlParameter("@SenderPhone", data.SenderPhone),
                new MySqlParameter("@SenderAddress", data.SenderAddress),
                new MySqlParameter("@Remark", data.Remarks),
                new MySqlParameter("@Weight", data.Weight),
                new MySqlParameter("@BatchNo", data.BatchNo),
                new MySqlParameter("@Status", OrderStatusEnum.Created),
                new MySqlParameter("@UserName", data.UserName)) > 0;
            if (insertStuatus)
                AddOrderStatus(data.OrderNumber, "1001");
            return true;
        }

        public static bool UpdateOrder(OrderInfo data)
        {
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection,
                              "select count(*) from ex_orderinfo where ID=@ID;",
                          new MySqlParameter("@ID", data.ID));
            if (Converter.TryToInt32(re) == 0)
            {
                throw new MsgException("快递单号不存在，请检查！");
            }

            bool updateStuatus = JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    @"UPDATE ex_orderinfo
                    SET
                        SENDER_PHONE = @SenderPhone,
                        SENDER_NAME = @SenderName,
                        SENDER_ADDRESS = @SenderAddress,
                        JBBW_PHONE = @JBBWPhone,
                        JBBW_NAME = @JBBWName,
                        JBBW_ADDRESS = @JBBWAddress,
                        REMARKS = @Remark,
                        WEIGHT = @Weight,
                        STATUS = @Status,
                        BATCH_NUMBER = @BatchNo,
                        UpdateTime = now(),
                        UpdateBy = @UserName
                    WHERE ID = @ID;",
                new MySqlParameter("@OrderNumber", data.OrderNumber),
                new MySqlParameter("@ID", data.ID),
                new MySqlParameter("@JBBWName", data.JBBWName),
                new MySqlParameter("@JBBWPhone", data.JBBWPhone),
                new MySqlParameter("@JBBWAddress", data.JBBWAddress),
                new MySqlParameter("@SenderName", data.SenderName),
                new MySqlParameter("@SenderPhone", data.SenderPhone),
                new MySqlParameter("@SenderAddress", data.SenderAddress),
                new MySqlParameter("@Remark", data.Remarks),
                new MySqlParameter("@Weight", data.Weight),
                new MySqlParameter("@BatchNo", data.BatchNo),
                new MySqlParameter("@Status", data.Status),
                new MySqlParameter("@UserName", data.UserName)) > 0;
            if (updateStuatus)
                AddOrderStatus(data.OrderNumber, data.Status);
            return true;
        }

        internal static bool UpdateStatusByBatchNumber(BatchUpdateParam batchParam)
        {
            // 根据action 获取 fromStatus、toStatus
            string fromStatus; string toStatus;
            switch (batchParam.Action)
            {
                case "gzconfirm":
                    fromStatus = ((int)OrderStatusEnum.Created).ToString();
                    toStatus = ((int)OrderStatusEnum.HasSend).ToString(); break;
                case "airportconfirm":
                    fromStatus = ((int)OrderStatusEnum.HasSend).ToString();
                    toStatus = ((int)OrderStatusEnum.InFlight).ToString(); break;
                case "jbbwconfirm":
                    fromStatus = ((int)OrderStatusEnum.InFlight).ToString();
                    toStatus = ((int)OrderStatusEnum.WaitDelivery).ToString(); break;
                default:
                    throw new MsgException("参数错误！");
            }

            // 根据batchNumber 查询订单信息
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, @" 
                        SELECT ORDER_NUM,ID FROM ex_orderinfo
                        WHERE BATCH_NUMBER=@BatchNumber and `STATUS`=@Status",
                        new MySqlParameter("@BatchNumber", batchParam.BatchNumber),
                        new MySqlParameter("@Status", fromStatus));

            if (dt != null && dt.Rows.Count > 0)
            {
                OrderStatusParam param = new OrderStatusParam();
                param.Status = toStatus;
                param.UserName = batchParam.UserName;
                param.dicOrders = new List<OrderStatusParam.OrderInfo_F>();
                foreach (DataRow row in dt.Rows)
                {
                    param.dicOrders.Add(new OrderStatusParam.OrderInfo_F()
                    {
                        Id = Converter.TryToString(row["ID"]),
                        Order_Num = Converter.TryToString(row["ORDER_NUM"])
                    });
                }
                // 更新订单信息
                return BatchUpdateStatus(param);
            }
            else
            {
                return true;
            }
        }

        internal static List<object> GetBatchNos()
        {
            string sql = @" SELECT DISTINCT BATCH_NUMBER FROM  ex_orderinfo";

            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, sql);
            List<object> result = new List<object>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string batchNo = Converter.TryToString(row[0]);
                    if (!string.IsNullOrEmpty(batchNo))
                    {
                        result.Add(new
                        {
                            label = batchNo,
                            value = batchNo
                        });
                    }
                }
            }
            return result;
        }

        internal static OrderInfo GetOrderDetail(OrderInfoParam searchParam)
        {
            OrderInfo orderInfo = new OrderInfo();
            string sql = @"SELECT `ID`,
                            `ORDER_NUM`,
                            `USER_ID`,
                            `SENDER_PHONE`,
                            `SENDER_NAME`,
                            `SENDER_ADDRESS`,
                            `JBBW_PHONE`,
                            `JBBW_NAME`,
                            `JBBW_ADDRESS`,
                            `REMARKS`,
                            `WEIGHT`,
                            `FLIGHT_NUM`,
                            `LANDING_TIME`,
                            `STATUS`,
                            `BATCH_NUMBER`,
                            `CreateTime`,
                            `CreatedBy`,
                            `UpdateTime`,
                            `UpdateBy`
                        FROM `ex_orderinfo`
                        {0};";
            List<MySqlParameter> param = new List<MySqlParameter>();

            string where;
            if (searchParam.ID != null)
            {
                where = " WHERE `ID` = @ID";
                param.Add(new MySqlParameter("@ID", searchParam.ID));
            }
            else if (!string.IsNullOrEmpty(searchParam.OrderNumber))
            {
                where = " WHERE ORDER_NUM = @OrderNumber";
                param.Add(new MySqlParameter("@OrderNumber", searchParam.OrderNumber));
            }
            else
            {
                throw new MsgException("参数错误！");
            }
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, string.Format(sql, where), param.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                OrderInfo orderinfo = new OrderInfo()
                {
                    ID = Converter.TryToInt64(row["ID"]),
                    OrderNumber = Converter.TryToString(row["ORDER_NUM"]),
                    SenderPhone = Converter.TryToString(row["SENDER_PHONE"]),
                    SenderName = Converter.TryToString(row["SENDER_NAME"]),
                    SenderAddress = Converter.TryToString(row["SENDER_ADDRESS"]),
                    JBBWPhone = Converter.TryToString(row["JBBW_PHONE"]),
                    JBBWName = Converter.TryToString(row["JBBW_NAME"]),
                    JBBWAddress = Converter.TryToString(row["JBBW_ADDRESS"]),
                    Remarks = Converter.TryToString(row["REMARKS"]),
                    Weight = Converter.TryToString(row["WEIGHT"]),
                    FlightNumber = Converter.TryToString(row["FLIGHT_NUM"]),
                    LandingTime = string.IsNullOrEmpty(Converter.TryToString(row["LANDING_TIME"])) ? "" : Converter.TryToDateTime(row["LANDING_TIME"]).ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = Converter.TryToString(row["STATUS"]),
                    StatusStr = OrderStatus.GetStatus(Converter.TryToString(row["STATUS"])),
                    BatchNo = Converter.TryToString(row["BATCH_NUMBER"]),
                    CreateTime = Converter.TryToDateTime(row["CreateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                    CreatedBy = Converter.TryToString(row["CreatedBy"]),
                    UpdateTime = Converter.TryToDateTime(row["UpdateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedBy = Converter.TryToString(row["UpdateBy"]),
                };
                orderinfo.BatchNo = string.IsNullOrEmpty(orderinfo.BatchNo) ? null : orderinfo.BatchNo;
                return orderinfo;
            }
            else
            {
                return null;
            }
        }

        internal static List<OrderInfo> GetOrderList(OrderInfoParam searchParam, out int total)
        {
            int offset = (searchParam.PageIndex - 1) * searchParam.PageSize;
            int rows = searchParam.PageSize;

            List<OrderInfo> recordList = new List<OrderInfo>();
            string sql = @" SELECT o.ID, ORDER_NUM, BATCH_NUMBER,JBBW_PHONE, JBBW_NAME,FLIGHT_NUM, LANDING_TIME, `STATUS`, o.CreateTime, e.ChineseName
                            FROM ex_orderinfo o
                            LEFT JOIN mt_employee e ON o.CreatedBy = e.UserName 
                            {0}
                            ORDER by o.CreateTime DESC
                            LIMIT {1},{2} ";

            string where = " WHERE 1=1 ";
            List<MySqlParameter> param = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(searchParam.OrderNumber))
            {
                where += " AND ORDER_NUM = @OrderNumber";
                param.Add(new MySqlParameter("@OrderNumber", searchParam.OrderNumber));
            }
            if (!string.IsNullOrEmpty(searchParam.Status))
            {
                where += " AND `STATUS` = @Status";
                param.Add(new MySqlParameter("@Status", searchParam.Status));
            }
            if (!string.IsNullOrEmpty(searchParam.CreateTimeStartStr))
            {
                where += " AND o.CreateTime > @CreateTimeStart";
                param.Add(new MySqlParameter("@CreateTimeStart", Convert.ToDateTime(searchParam.CreateTimeStartStr)));
            }
            if (!string.IsNullOrEmpty(searchParam.CreateTimeEndStr))
            {
                where += " AND o.CreateTime < @CreateTimeEnd";
                param.Add(new MySqlParameter("@CreateTimeEnd", Convert.ToDateTime(searchParam.CreateTimeEndStr)));
            }
            if (!string.IsNullOrEmpty(searchParam.BatchNo))
            {
                where += " AND BATCH_NUMBER = @BatchNo";
                param.Add(new MySqlParameter("@BatchNo", searchParam.BatchNo));
            }
            if (!string.IsNullOrEmpty(searchParam.FlightNumber))
            {
                where += " AND FLIGHT_NUM = @FlightNumber";
                param.Add(new MySqlParameter("@FlightNumber", searchParam.FlightNumber));
            }
            if (!string.IsNullOrEmpty(searchParam.KeyWord))
            {
                where += " AND JBBW_PHONE like CONCAT('%',@KeyWord,'%') OR JBBW_NAME like CONCAT('%',@KeyWord,'%')";
                param.Add(new MySqlParameter("@KeyWord", searchParam.KeyWord));
            }

            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, string.Format(sql, where, offset, rows), param.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    recordList.Add(new OrderInfo()
                    {
                        ID = Converter.TryToInt64(row["ID"]),
                        OrderNumber = Converter.TryToString(row["ORDER_NUM"]),
                        BatchNo = Converter.TryToString(row["BATCH_NUMBER"]),
                        JBBWPhone = Converter.TryToString(row["JBBW_PHONE"]),
                        JBBWName = Converter.TryToString(row["JBBW_NAME"]),
                        FlightNumber = Converter.TryToString(row["FLIGHT_NUM"]),
                        LandingTime = string.IsNullOrEmpty(Converter.TryToString(row["LANDING_TIME"])) ? "" : Converter.TryToDateTime(row["LANDING_TIME"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = Converter.TryToString(row["STATUS"]),
                        StatusStr = OrderStatus.GetStatus(Converter.TryToString(row["STATUS"])),
                        CreateTime = Converter.TryToDateTime(row["CreateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        CreatedBy = Converter.TryToString(row["ChineseName"])
                    });
                }
            }

            // 查询总数
            string sqlCount = @"SELECT count(*) FROM ex_orderinfo o {0}";
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection, string.Format(sqlCount, where), param.ToArray());
            total = Convert.ToInt32(re);

            return recordList;
        }

        /// <summary>
        /// 修改订单状态
        /// </summary>
        /// <param name="dicOrders">订单id,订单号</param>
        /// <param name="status">状态</param>
        /// <param name="userName">用户名</param>
        /// <returns></returns>
        public static bool BatchUpdateStatus(OrderStatusParam param)
        {
            string ids = string.Join(",", param.dicOrders.Select(t => t.Id).ToList());
            bool upsucess = JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                     $@"UPDATE ex_orderinfo
                            SET
                                STATUS = @Status,
                                UpdateBy = @UserName,
                                UpdateTime = now()
                            WHERE ID IN ({ids});",
                 new MySqlParameter("@Status", param.Status),
                 new MySqlParameter("@UserName", param.UserName)) > 0;
            if (upsucess)
            {
                foreach (var item in param.dicOrders)
                {
                    AddOrderStatus(item.Order_Num, param.Status);
                }
            }
            return true;
        }

        internal static bool AddOrderStatus(string orderNum, string status)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    $@"INSERT INTO `ex_statusinfo`(`ORDER_NUM`, `UPDATE_TIME`, `UPDATE_STATUS`, `REMARKS`) 
                           VALUES (@ORDER_NUM, now(), @UPDATE_STATUS, @REMARKS);",
                new MySqlParameter("@ORDER_NUM", orderNum),
                new MySqlParameter("@UPDATE_STATUS", OrderStatus.GetStatus(status)),
                new MySqlParameter("@REMARKS", OrderStatusDetaill.GetStatus(status)));
            return true;
        }
    }
}
