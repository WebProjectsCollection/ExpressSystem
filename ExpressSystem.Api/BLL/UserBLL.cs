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
    public class UserBLL
    {
        public static UserInfo GetUserDetail(string ntid, string siteId)
        {
            List<Object> userList = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(
                Config.DBConnection,
                @"SELECT
                    e.*,
                    ur.NTID,
                    r.RoleName,
                    r.RoleId,
                    r.SiteId
                FROM cf_userrole ur
                INNER JOIN cf_role r ON ur.RoleID = r.roleid AND r.SiteID=@SiteID
                INNER JOIN mt_employee e ON ur.EmployeeID=e.EmployeeID
                WHERE ur.NTID = @NTID LIMIT 1",
                new MySqlParameter("@NTID", ntid),
                new MySqlParameter("@SiteID", siteId));

            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new UserInfo
                {
                    EmployeeID = Converter.TryToInt64(row["EmployeeID"]),
                    NTID = Converter.TryToString(row["NTID"]),
                    ChineseName = Converter.TryToString(row["ChineseName"]),
                    EmailAddress = Converter.TryToString(row["EmailAddress"]),
                    Department = Converter.TryToString(row["Department"]),
                    RoleName = Converter.TryToString(row["RoleName"]),
                    RoleId = Converter.TryToInt32(row["RoleId"]),
                    SiteId = Converter.TryToInt32(row["SiteId"]),
                };
            }
            else
            {
                return null;
            }
        }

        internal static List<object> GetapplyRecords(long employeeId)
        {
            DataTable tbRecord = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                @"SELECT a.`*`, IFNULL(r.BackNumber,0) AS BackNumber
                FROM view_applyrecord a
                LEFT JOIN ( SELECT GiveoutRecordID, SUM(BackNumber) AS BackNumber FROM cf_sendbackrecord GROUP BY GiveoutRecordID ) r ON a.ID=r.giveoutRecordID
                WHERE ToEmployeeId=@EmployeeId ORDER BY a.OutTime DESC",
                new MySqlParameter("@EmployeeId", employeeId));
            List<object> recordList = new List<object>();
            foreach (DataRow sRow in tbRecord.Rows)
            {
                DateTime recordTime = Converter.TryToDateTime(sRow["OutTime"]);
                TimeSpan diffSpan = DateTime.Now.Subtract(recordTime);
                int status = 0;
                if (diffSpan.TotalDays < 90)
                {
                    status = 1; // day<90
                }
                else if (90 <= diffSpan.TotalDays && diffSpan.TotalDays <= 360)
                {
                    status = 2; // 90<=day<=360
                }
                else
                {
                    status = 3; // day>360
                }

                recordList.Add(new
                {
                    ID = Converter.TryToInt64(sRow["ID"]),
                    Season = Converter.TryToString(sRow["UniformSeason"]),
                    Style = Converter.TryToString(sRow["UniformStyle"]),
                    Price = Converter.TryToDecimal(sRow["UniformPrice"]),
                    Number = Converter.TryToInt32(sRow["Number"]),
                    BackNumber = Converter.TryToInt32(sRow["BackNumber"]),
                    CreateTime = recordTime,
                    Status = status,
                });
            }
            return recordList;
        }

        internal static object GetUserStockInfo(string badgeId)
        {
            DataTable dt = JabMySqlHelper.ExecuteDataTable(
                Config.DBConnection,
                @"SELECT e.EmployeeID,e.ChineseName FROM mt_employee e 
                INNER JOIN cf_userrole ur ON e.employeeId=ur.EmployeeID
                INNER JOIN	 cf_role r ON r.RoleID=ur.RoleID AND r.RoleCode='departmentAdmin'
                WHERE e.BadgeID=@BadgeID  AND e.employmentStatus=1 LIMIT 1",
                new MySqlParameter("@BadgeID", badgeId));

            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                long employeeID = Converter.TryToInt64(row["EmployeeID"]);
                DataTable tbStock = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                    @"SELECT * from view_employeestock WHERE EmployeeId=@EmployeeId and stock>0",
                    new MySqlParameter("@EmployeeId", employeeID));
                List<object> stockList = new List<object>();
                foreach (DataRow sRow in tbStock.Rows)
                {
                    stockList.Add(new
                    {
                        Season = Converter.TryToString(sRow["UniformSeason"]),
                        Style = Converter.TryToString(sRow["UniformStyle"]),
                        Stock = Converter.TryToInt32(sRow["Stock"]),
                    });
                }

                return new
                {
                    EmployeeID = Converter.TryToInt64(row["EmployeeID"]),
                    ChineseName = Converter.TryToString(row["ChineseName"]),
                    StockList = stockList
                };
            }
            else
            {
                throw new MsgException("该用户不存在，或不是部门管理员！");
            }
        }

        internal static object GetUserInfoById(string badgeId, string employeeId)
        {
            List<Object> userList = new List<Object>();
            string sql = "SELECT * From mt_employee WHERE 1=1 {0} LIMIT 1";
            string where = "";
            List<MySqlParameter> paramList = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(badgeId))
            {
                where += " AND BadgeID=@BadgeID AND employmentStatus=1 ";
                paramList.Add(new MySqlParameter("@BadgeID", badgeId));
            }
            if (!string.IsNullOrEmpty(employeeId))
            {
                where += " AND EmployeeID=@EmployeeID ";
                paramList.Add(new MySqlParameter("@EmployeeID", employeeId));
            }
            // 查询条件为空，返回null
            if (where.IsNullOrEmpty())
            {
                return null;
            }
            DataTable dt = JabMySqlHelper.ExecuteDataTable(
                Config.DBConnection,
                string.Format(sql, where),
                paramList.ToArray());

            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new
                {
                    EmployeeID = Converter.TryToInt32(row["EmployeeID"]),
                    ChineseName = Converter.TryToString(row["ChineseName"]),
                    EmailAddress = Converter.TryToString(row["EmailAddress"]),
                    Department = Converter.TryToString(row["Department"]),
                    Costcenter = Converter.TryToString(row["Costcenter"]),
                };
            }
            else
            {
                return null;
            }
        }

        public static List<Object> GetUserList(int siteId)
        {
            List<Object> userList = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(
                Config.DBConnection,
                @"SELECT 
                    e.*,
                    ur.NTID,
                    r.RoleName,
                    r.RoleId
                FROM cf_userrole ur
                INNER JOIN cf_role r ON ur.RoleID = r.roleid AND r.SiteID=@SiteID
                INNER JOIN mt_employee e ON ur.EmployeeID=e.EmployeeID",
                new MySqlParameter("@SiteID", siteId));

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    userList.Add(new
                    {
                        EmployeeID = Converter.TryToInt32(row["EmployeeID"]),
                        NTID = Converter.TryToString(row["NTID"]),
                        ChineseName = Converter.TryToString(row["ChineseName"]),
                        EmailAddress = Converter.TryToString(row["EmailAddress"]),
                        Department = Converter.TryToString(row["Department"]),
                        RoleName = Converter.TryToString(row["RoleName"]),
                        RoleId = Converter.TryToInt32(row["RoleId"]),
                    });
                }
            }

            return userList;
        }

        internal static bool SaveUser(string ntid, string employeeId, int roleId)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    "Update cf_userrole set RoleID=@RoleID where EmployeeID=@EmployeeID and NTID=@NTID;",
                new MySqlParameter("@EmployeeID", employeeId),
                new MySqlParameter("@NTID", ntid),
                new MySqlParameter("@RoleID", roleId));
            return true;
        }

        internal static bool DeleteUser(string ntid)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    "Delete from cf_userrole where NTID=@NTID;",
                new MySqlParameter("@NTID", ntid));
            return true;
        }

        public static bool AddNewUser(string ntid, string employeeId, int roleId)
        {
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection,
                              "select count(*) from cf_userrole where EmployeeID=@EmployeeID and NTID=@NTID;",
                          new MySqlParameter("@EmployeeID", employeeId),
                          new MySqlParameter("@NTID", ntid));
            if (Converter.TryToInt32(re) > 0)
            {
                throw new MsgException("用户已存在");
            }

            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    "INSERT INTO cf_userrole (EmployeeID, NTID,RoleID) VALUES (@EmployeeID,@NTID,@RoleID);",
                new MySqlParameter("@EmployeeID", employeeId),
                new MySqlParameter("@NTID", ntid),
                new MySqlParameter("@RoleID", roleId));
            return true;
        }

        public static UserInfo GetUserByRole(string employeeId, int siteId, string roleCode)
        {
            DataTable dt = JabMySqlHelper.ExecuteDataTable(
               Config.DBConnection,
               @"SELECT e.EmployeeID,e.ChineseName FROM mt_employee e 
                    INNER JOIN cf_userrole ur ON e.employeeId=ur.EmployeeID
                    INNER JOIN	 cf_role r ON r.RoleID=ur.RoleID AND r.RoleCode=@roleCode
                WHERE e.employeeId=@employeeId  AND e.employmentStatus=1 AND r.siteId=@siteId LIMIT 1",
              new MySqlParameter[] { new MySqlParameter("@employeeId", employeeId),
                  new MySqlParameter("@roleCode", roleCode),
                  new MySqlParameter("@siteId", siteId) });

            if (dt == null || dt.Rows.Count == 0)
            {
                throw new MsgException("该用户不存在，或不是部门管理员！");
            }
            else
            {
                DataRow row = dt.Rows[0];
                return new UserInfo
                {
                    EmployeeID = Converter.TryToInt64(row["EmployeeID"]),
                    ChineseName = Converter.TryToString(row["ChineseName"])
                };
            }
        }
    }
}
