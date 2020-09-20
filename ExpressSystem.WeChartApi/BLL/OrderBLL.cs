using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ExpressSystem.WeChartApi.Entity;
using ExpressSystem.WeChartApi.Utilities;

namespace ExpressSystem.WeChartApi.BLL
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

        internal static List<StatusInfo> GetOrderInfo(string order_num)
        {
            List<StatusInfo> statusInfos = new List<StatusInfo>();
            string sql = @"SELECT 
	                                    ORDER_NUM,
	                                    UPDATE_TIME,
	                                    UPDATE_STATUS,
	                                    REMARKS
                                    FROM 
	                                    ex_statusinfo
                                    WHERE 
	                                    ORDER_NUM = @OrderNumber
                                    ORDER BY
                                        UPDATE_TIME";
            List<MySqlParameter> param = new List<MySqlParameter>();
            param.Add(new MySqlParameter("@OrderNumber", order_num));
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, sql, param.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    StatusInfo statusInfo = new StatusInfo()
                    {
                        Order_Num = Converter.TryToString(row["ORDER_NUM"]),
                        Update_Status = Converter.TryToString(row["UPDATE_STATUS"]),
                        Update_Date = Converter.TryToDateTime(row["UPDATE_TIME"]).ToShortDateString(),
                        Update_Time = Converter.TryToDateTime(row["UPDATE_TIME"]).ToString("HH:mm:ss"),
                        Remarks = Converter.TryToString(row["REMARKS"]),
                    };
                    statusInfos.Add(statusInfo);
                }
                statusInfos.Last().LastFlag = true;
            }
            return statusInfos;
        }
    }
}
