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
            string password = Config.DefaultPassword.ToMD5().ToMD5();

            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    $"INSERT INTO ex_orderinfo (ORDER_NUM,JBBW_NAME,JBBW_PHONE,JBBW_ADDRESS,SENDER_NAME,SENDER_PHONE,SENDER_ADDRESS,STATUS,REMARKS,WEIGHT,BATCH_NUMBER,CreatedBy) " +
                    $"VALUES (@OrderNumber,@JBBWName,@JBBWPhone,@JBBWAddress,@SenderName,@SenderPhone,@SenderAddress,@Status,@Remark,@Weight,@BatchNo,@UserName);",
                new MySqlParameter("@OrderNumber", data.OrderNumber),
                new MySqlParameter("@JBBWName", data.JBBWName),
                new MySqlParameter("@JBBWPhone", data.JBBWPhone),
                new MySqlParameter("@JBBWAddress", data.JBBWAddress),
                new MySqlParameter("@SenderName", data.SenderName),
                new MySqlParameter("@SenderPhone", data.SenderPhone),
                new MySqlParameter("@SenderAddress", data.SenderAddress),
                new MySqlParameter("@Remark", data.Remark),
                new MySqlParameter("@Weight", data.Weight),
                new MySqlParameter("@BatchNo", data.BatchNo),
                new MySqlParameter("@Status", OrderStatusEnum.Created),
                new MySqlParameter("@UserName", data.UserName));
            return true;
        }

        internal static List<object> GetBatchNos()
        {
            string sql = @" SELECT DISTINCT BATCH_NUMBER FROM  ex_orderinfo
                            ORDER by CreateTime DESC ";

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

        internal static List<OrderInfo> GetOrderList(OrderInfoParam searchParam, out int total)
        {
            int offset = (searchParam.PageIndex - 1) * searchParam.PageSize;
            int rows = searchParam.PageSize;

            List<OrderInfo> recordList = new List<OrderInfo>();
            string sql = @" SELECT o.ID, ORDER_NUM, JBBW_PHONE, JBBW_NAME,FLIGHT_NUM, LANDING_TIME, `STATUS`, o.CreateTime, e.ChineseName
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
                        JBBWPhone = Converter.TryToString(row["JBBW_PHONE"]),
                        JBBWName = Converter.TryToString(row["JBBW_NAME"]),
                        FlightNumber = Converter.TryToString(row["FLIGHT_NUM"]),
                        LandingTime = string.IsNullOrEmpty(Converter.TryToString(row["LANDING_TIME"])) ? "" : Converter.TryToDateTime(row["LANDING_TIME"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = OrderStatus.GetStatus(Converter.TryToString(row["STATUS"])),
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
    }
}
