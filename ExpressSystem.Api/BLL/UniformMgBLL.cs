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
    public class UniformMgBLL
    {
        public static bool Giveout(int siteId, int type, long fromEmployeeId, long toEmployeeId, int number, List<UniformType> uniformList)
        {
            if (type == 2)
            {
                DataTable dt = JabMySqlHelper.ExecuteDataTable(
                Config.DBConnection,
                @"SELECT e.employeeId FROM mt_employee e 
                    INNER JOIN cf_userrole ur ON e.employeeId=ur.EmployeeID
                    INNER JOIN cf_role r ON r.RoleID=ur.RoleID AND r.RoleCode='departmentAdmin'
                WHERE ur.EmployeeID=@EmployeeId  AND e.employmentStatus=1",
                new MySqlParameter("@EmployeeId", toEmployeeId));

                if (dt == null || dt.Rows.Count == 0)
                {
                    throw new MsgException("该用户不存在，或不是部门管理员！");
                }
            }

            if (uniformList != null && uniformList.Count > 0)
            {
                foreach (UniformType item in uniformList)
                {
                    string remark = string.Format("{0}{1}从{2}处领取工衣：{3}共计{4}套",
                        type == 1 ? "员工" : "部门管理员",
                        toEmployeeId,
                        fromEmployeeId == -1 ? "系统" : "部门管理员" + fromEmployeeId,
                        item.Style,
                        number
                        );

                    JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                        @"
                        INSERT INTO cf_giveoutrecord (SiteID, FromEmployeeID, ToEmployeeID, UniformSeason, UniformStyle, UniformPrice, Number, Type, Remark) 
                        VALUES (@SiteID, @FromEmployeeID, @ToEmployeeID, @UniformSeason, @UniformStyle, @UniformPrice, @Number, @Type, @Remark);
                        ",
                    new MySqlParameter("@SiteID", siteId),
                    new MySqlParameter("@FromEmployeeID", fromEmployeeId),
                    new MySqlParameter("@ToEmployeeID", toEmployeeId),
                    new MySqlParameter("@UniformSeason", item.Season),
                    new MySqlParameter("@UniformStyle", item.Style),
                    new MySqlParameter("@UniformPrice", item.Price),
                    new MySqlParameter("@Number", number),
                    new MySqlParameter("@Type", type),
                    new MySqlParameter("@Remark", remark));
                }

                return true;
            }
            else
            {
                throw new MsgException("工服信息错误！");
            }
        }

        internal static void Transfer(int siteId, long fromEmployeeId, long toEmployeeId, List<UniformType> uniformList)
        {
            if (uniformList != null && uniformList.Count > 0)
            {
                foreach (UniformType item in uniformList)
                {
                    item.Price = 0;

                    //1、将工服退还给系统
                    string fromRemark = string.Format("部门管理员{0}向系统退还工衣：{1}共计{2}套", fromEmployeeId, item.Style, item.Stock);

                    JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                        @"
                        INSERT INTO cf_giveoutrecord (SiteID, FromEmployeeID, ToEmployeeID, UniformSeason, UniformStyle, UniformPrice, Number, Type, Remark) 
                        VALUES (@SiteID, @FromEmployeeID, @ToEmployeeID, @UniformSeason, @UniformStyle, @UniformPrice, @Number, @Type, @Remark);
                        ",
                    new MySqlParameter("@SiteID", siteId),
                    new MySqlParameter("@FromEmployeeID", fromEmployeeId),
                    new MySqlParameter("@ToEmployeeID", -1),
                    new MySqlParameter("@UniformSeason", item.Season),
                    new MySqlParameter("@UniformStyle", item.Style),
                    new MySqlParameter("@UniformPrice", item.Price),
                    new MySqlParameter("@Number", item.Stock),
                    new MySqlParameter("@Type", 3),// 3-部门管理员退还给系统
                    new MySqlParameter("@Remark", fromRemark));

                    //2、系统重新发放工服
                    string remark = string.Format("部门管理员{0}从系统处领取工衣：{1}共计{2}套", toEmployeeId, item.Style, item.Stock);

                    JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                        @"
                        INSERT INTO cf_giveoutrecord (SiteID, FromEmployeeID, ToEmployeeID, UniformSeason, UniformStyle, UniformPrice, Number, Type, Remark) 
                        VALUES (@SiteID, @FromEmployeeID, @ToEmployeeID, @UniformSeason, @UniformStyle, @UniformPrice, @Number, @Type, @Remark);
                        ",
                    new MySqlParameter("@SiteID", siteId),
                    new MySqlParameter("@FromEmployeeID", -1),
                    new MySqlParameter("@ToEmployeeID", toEmployeeId),
                    new MySqlParameter("@UniformSeason", item.Season),
                    new MySqlParameter("@UniformStyle", item.Style),
                    new MySqlParameter("@UniformPrice", item.Price),
                    new MySqlParameter("@Number", item.Stock),
                    new MySqlParameter("@Type", 2), //2-发放给部门
                    new MySqlParameter("@Remark", remark));
                }
            }
        }

        internal static List<DebtRecord> ChargeRecord(RecordSearchParam searchParam, out int total)
        {
            int offset = (searchParam.PageIndex - 1) * searchParam.PageSize;
            int rows = searchParam.PageSize;

            List<DebtRecord> recordList = new List<DebtRecord>();
            string sql = @" SELECT EmployeeID, ChineseName, Department, InDate, LeaveDate, EmploymentStatus,Costcenter, sum(debt) as Debt, GROUP_CONCAT(CONCAT(uniformstyle,`Number`,N'套:', ROUND(debt, 2))) as Details
                            FROM
                                (SELECT EmployeeID, ChineseName, Department, InDate, LeaveDate, EmploymentStatus, UniformStyle,sum(`Number`) as `Number`, sum(debt) as debt, Costcenter
                                FROM (SELECT EmployeeID, ChineseName, EmploymentStatus, Department, Costcenter, CreateDate AS InDate, LastUpdate AS LeaveDate FROM mt_employee {1}) AS e
	                            INNER JOIN (SELECT ToEmployeeID, (`Number` - backNumber) as `Number`, UniformPrice, UniformStyle,
						                            CASE
							                            WHEN DATEDIFF(NOW(), outTIme) >= 365 THEN uniformPrice * 60 / 100 * (`Number` - backNumber)
							                            ELSE uniformPrice * (`Number` - backNumber)
						                            END AS debt
				                            FROM view_applyrecord
				                            WHERE backnumber < number AND DATEDIFF(NOW(), outTIme) <= 365 {0}
                                            ) AS r 
	                            ON e.EmployeeID = r.ToemployeeId
	                            GROUP BY employeeid , uniformstyle
                                ) AS t
                            GROUP BY employeeid LIMIT {2},{3} ";

            string where = " AND SiteID=@SiteID";
            string where2 = " WHERE 1=1 ";
            List<MySqlParameter> param = new List<MySqlParameter>();
            param.Add(new MySqlParameter("@SiteID", searchParam.SiteId));

            if (!string.IsNullOrEmpty(searchParam.EmployeeId))
            {
                where += " AND ToEmployeeID = @EmployeeId";
                param.Add(new MySqlParameter("@EmployeeId", Converter.TryToInt64(searchParam.EmployeeId)));
            }
            if (!string.IsNullOrEmpty(searchParam.Status))
            {
                where2 += " AND EmploymentStatus = @EmploymentStatus";
                param.Add(new MySqlParameter("@EmploymentStatus", Converter.TryToInt32(searchParam.Status)));
            }
            if (!string.IsNullOrEmpty(searchParam.OutTimeStartStr))
            {
                where2 += " AND LastUpdate > @OutTimeStart";
                param.Add(new MySqlParameter("@OutTimeStart", Convert.ToDateTime(searchParam.OutTimeStartStr)));
            }
            if (!string.IsNullOrEmpty(searchParam.OutTimeEndStr))
            {
                where2 += " AND LastUpdate < @OutTimeEnd";
                param.Add(new MySqlParameter("@OutTimeEnd", Convert.ToDateTime(searchParam.OutTimeEndStr)));
            }

            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, string.Format(sql, where, where2, offset, rows), param.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var employmentStatus = Converter.TryToInt32(row["EmploymentStatus"]);
                    recordList.Add(new DebtRecord()
                    {
                        ChineseName = Converter.TryToString(row["ChineseName"]),
                        EmployeeID = Converter.TryToInt64(row["EmployeeID"]),
                        Department = Converter.TryToString(row["Department"]),
                        Costcenter = Converter.TryToString(row["Costcenter"]),
                        InDate = Converter.TryToDateTime(row["InDate"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        LeaveDate = employmentStatus == 1 ? "--" : Converter.TryToDateTime(row["LeaveDate"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        Debt = Converter.TryToDecimal(row["Debt"]),
                        Details = Converter.TryToString(row["Details"]),
                    });
                }
            }

            // 查询总数
            string sqlCount = @"SELECT count(DISTINCT ToEmPloyeeID) AS total FROM	
                                (SELECT ToEmPloyeeID 
	                                FROM view_applyrecord 
	                                WHERE backnumber<number AND DATEDIFF(NOW(),outTIme)<=365 {0}
                                ) a INNER JOIN	mt_employee b ON b.EmployeeID=a.ToEmPloyeeID {1}";
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection, string.Format(sqlCount, where, where2), param.ToArray());
            total = Convert.ToInt32(re);

            return recordList;
        }

        public static List<Object> OutAndInRecord(RecordSearchParam searchParam, out int total)
        {
            int offset = (searchParam.PageIndex - 1) * searchParam.PageSize;
            int rows = searchParam.PageSize;

            List<Object> recordList = new List<Object>();
            string sql = @"SELECT 
                                e.ChineseName,
                                e.EmployeeID,
                                e.Department,
                                r.FromEmployeeID,
                                e2.ChineseName as FromChineseName,
                                e.Costcenter,
                                r.UniformStyle,
                                r.Number,
                                r.OutTime,
                                r.BackNumber,
                                r.BackTime
                            FROM view_applyrecord r 
                            INNER JOIN mt_employee e ON r.ToEmployeeID = e.EmployeeID
                            LEFT JOIN mt_employee e2 ON r.FromEmployeeID = e2.EmployeeID
                            WHERE r.SiteID=@SiteID {0}
                            ORDER BY r.OutTime DESC
                            LIMIT {1},{2}";
            string where = "";
            List<MySqlParameter> param = new List<MySqlParameter>();
            param.Add(new MySqlParameter("@SiteID", searchParam.SiteId));

            if (!string.IsNullOrEmpty(searchParam.OutTimeStartStr))
            {
                where += " AND OutTime > @OutTimeStart";
                param.Add(new MySqlParameter("@OutTimeStart", Convert.ToDateTime(searchParam.OutTimeStartStr)));
            }
            if (!string.IsNullOrEmpty(searchParam.OutTimeEndStr))
            {
                where += " AND OutTime < @OutTimeEnd";
                param.Add(new MySqlParameter("@OutTimeEnd", Convert.ToDateTime(searchParam.OutTimeEndStr)));
            }
            if (!string.IsNullOrEmpty(searchParam.BackTimeStartStr))
            {
                where += " AND BackTime > @BackTimeStart";
                param.Add(new MySqlParameter("@BackTimeStart", Convert.ToDateTime(searchParam.BackTimeStartStr)));
            }
            if (!string.IsNullOrEmpty(searchParam.BackTimeEndStr))
            {
                where += " AND BackTime < @BackTimeEnd";
                param.Add(new MySqlParameter("@BackTimeEnd", Convert.ToDateTime(searchParam.BackTimeEndStr)));
            }
            if (!string.IsNullOrEmpty(searchParam.EmployeeId))
            {
                where += " AND e.EmployeeID = @EmployeeId";
                param.Add(new MySqlParameter("@EmployeeId", Converter.TryToInt64(searchParam.EmployeeId)));
            }

            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, string.Format(sql, where, offset, rows), param.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string fromEmployeeID = Converter.TryToString(row["FromEmployeeID"]);
                    string fromChineseName = Converter.TryToString(row["FromChineseName"]);
                    string fromEmployee;
                    if (string.IsNullOrEmpty(fromEmployeeID))
                    {
                        fromEmployee = "--";
                    }
                    else if (fromEmployeeID == "-1")
                    {
                        fromEmployee = "系统";
                    }
                    else
                    {
                        fromEmployee = $"{fromEmployeeID}({fromChineseName})";
                    }
                    recordList.Add(new
                    {
                        ChineseName = Converter.TryToString(row["ChineseName"]),
                        EmployeeID = Converter.TryToString(row["EmployeeID"]),
                        Department = Converter.TryToString(row["Department"]),
                        FromEmployee = fromEmployee,
                        Costcenter = Converter.TryToString(row["Costcenter"]),
                        UniformStyle = Converter.TryToString(row["UniformStyle"]),
                        Number = Converter.TryToInt32(row["Number"]),
                        OutTime = Converter.TryToDateTime(row["OutTime"]),
                        BackNumber = Converter.TryToInt32(row["BackNumber"]),
                        BackTime = row["BackTime"] == null ? null : (DateTime?)Converter.TryToDateTime(row["BackTime"]),
                    });
                }
            }

            // 查询总数
            string sqlCount = @"SELECT count(*) FROM view_applyrecord r
                                INNER JOIN mt_employee e ON r.ToEmployeeID = e.EmployeeID
                                WHERE r.SiteID = @SiteID {0}";
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection, string.Format(sqlCount, where), param.ToArray());
            total = Convert.ToInt32(re);

            return recordList;
        }

        public static bool Sendback(long recordId, int backNumber)
        {
            // 验证数据有效性
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection,
                @"SELECT count(id) from cf_giveoutrecord WHERE id=@ID",
                new MySqlParameter("@id", recordId));
            if (Converter.TryToInt32(re) == 0)
            {
                throw new MsgException("数据错误！请刷新页面后重试");
            }

            // 插入退还记录
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                        @" INSERT INTO cf_sendbackrecord (GiveoutRecordID, BackNumber) VALUES(@GiveoutRecordID, @BackNumber); ",
                    new MySqlParameter("@GiveoutRecordID", recordId),
                    new MySqlParameter("@BackNumber", backNumber));

            return true;
        }

        internal static List<Object> ManagerStockList(RecordSearchParam searchParam)
        {
            List<Object> list = new List<Object>();
            string sql = @"SELECT a.EmployeeID, b.ChineseName,b.Department,b.Costcenter,UniformSeason,UniformStyle,Total
                            FROM (
                            SELECT employeeId,uniformSeason,uniformstyle, SUM(stock) AS total FROM view_employeestock
                            WHERE {0} GROUP BY employeeId,uniformSeason,uniformstyle HAVING total>0 AND employeeId!=-1
                            ) a LEFT JOIN mt_employee b ON a.employeeId = b.employeeId
                            ORDER BY employeeid";

            string where = " SiteID=@SiteID";
            List<MySqlParameter> param = new List<MySqlParameter>();
            param.Add(new MySqlParameter("@SiteID", searchParam.SiteId));

            if (!string.IsNullOrEmpty(searchParam.EmployeeId))
            {
                where += " AND employeeId = @EmployeeId";
                param.Add(new MySqlParameter("@EmployeeId", Converter.TryToInt64(searchParam.EmployeeId)));
            }

            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, string.Format(sql, where), param.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        ChineseName = Converter.TryToString(row["ChineseName"]),
                        EmployeeID = Converter.TryToInt64(row["EmployeeID"]),
                        Department = Converter.TryToString(row["Department"]),
                        Costcenter = Converter.TryToString(row["Costcenter"]),
                        UniformSeason = Converter.TryToString(row["UniformSeason"]),
                        UniformStyle = Converter.TryToString(row["UniformStyle"]),
                        Total = Converter.TryToInt64(row["Total"]),
                    });
                }

            }
            return list;
        }

    }
}
