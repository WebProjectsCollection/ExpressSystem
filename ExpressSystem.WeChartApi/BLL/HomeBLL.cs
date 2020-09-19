using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ExpressSystem.WeChartApi.Utilities;

namespace ExpressSystem.WeChartApi.BLL
{
    public class HomeBLL
    {
        internal static List<object> GetUniformSummary(int siteId, DateTime startTime, DateTime endTime)
        {
            List<Object> list = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                @"
                SELECT u.Style,OutSummary,InSummary
                FROM ( SELECT DISTINCT style FROM cf_uniformtype WHERE SiteID = @SiteID) as u
                LEFT JOIN(   
                    SELECT uniformstyle,SUM(NUMBER) AS outSummary 
                    FROM cf_giveoutrecord 
                    WHERE TYPE = 1 AND siteID = @SiteID AND CreateTime > @StartTime AND CreateTime < @EndTime
                    GROUP BY uniformstyle
                ) AS a ON u.style = a.uniformstyle
                LEFT JOIN(
                    SELECT uniformstyle, SUM(backnumber) AS inSummary
                    FROM view_applyrecord
                    WHERE siteId = @SiteID AND backNumber > 0 AND backTime > @StartTime AND backTime < @EndTime
                    GROUP BY uniformstyle
                ) AS b ON u.style =  b.uniformstyle",
                new MySqlParameter("@SiteID", siteId),
                new MySqlParameter("@StartTime", startTime),
                new MySqlParameter("@EndTime", endTime));

            if (dt != null && dt.Rows.Count > 0)
            {
                int sumOut = 0;
                int sumIn = 0;
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        UniformStyle = Converter.TryToString(row["Style"]),
                        OutSummary = Converter.TryToInt32(row["OutSummary"]),
                        InSummary = Converter.TryToInt32(row["InSummary"])
                    });
                    sumOut += Converter.TryToInt32(row["OutSummary"]);
                    sumIn += Converter.TryToInt32(row["InSummary"]);
                }
                list.Add(new
                {
                    UniformStyle = "Total",
                    OutSummary = sumOut,
                    InSummary = sumIn
                });
            }

            return list;
        }

        internal static List<object> GetEmployeeSummary(int siteId)
        {
            List<Object> list = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                @"                
                    select sum(isAdd) as addSum,sum(isLeave) AS leaveSum,sum(t) as total from(
                    SELECT 
                        IF(TO_DAYS(createdate) = TO_DAYS(NOW()) AND EmploymentStatus = 1,
                            1,
                            0) AS isAdd, 
                        IF(TO_DAYS(LastUpdate) = TO_DAYS(NOW()) AND EmploymentStatus = 0,
                            1,
                            0) AS isLeave, 
                        IF(EmploymentStatus = 1,
                            1,
                            0) AS t
                    FROM
                        mt_employee) as temp");

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        AddSum = Converter.TryToInt32(row["addSum"]),
                        LeaveSum = Converter.TryToInt32(row["leaveSum"]),
                        Total = Converter.TryToInt32(row["total"])
                    });
                }
            }

            return list;
        }
    }
}
