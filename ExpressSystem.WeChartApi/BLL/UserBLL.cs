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
    public class UserBLL
    {
        public static UserInfo GetUserDetail(string userName, string password)
        {
            password = password.ToUpper().ToMD5();
            List<Object> userList = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(
                Config.DBConnection,
                @"SELECT
                    e.UserName,e.ChineseName,e.WaitSet,
                    r.RoleName,
                    r.RoleId
                FROM mt_employee e
                INNER JOIN cf_role r ON e.RoleID = r.roleid 
                WHERE e.UserName = @UserName and e.Password = @Password LIMIT 1",
                new MySqlParameter("@UserName", userName),
                new MySqlParameter("@Password", password));

            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new UserInfo
                {
                    UserName = Converter.TryToInt64(row["UserName"]),
                    ChineseName = Converter.TryToString(row["ChineseName"]),
                    RoleName = Converter.TryToString(row["RoleName"]),
                    RoleId = Converter.TryToInt32(row["RoleId"]),
                    WaitSet = Converter.TryToInt16(row["WaitSet"]),
                };
            }
            else
            {
                return null;
            }
        }


        public static List<Object> GetUserList()
        {
            List<Object> userList = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(
                Config.DBConnection,
                @"SELECT 
                    e.UserName,e.ChineseName,e.RoleId,
                    r.RoleName
                FROM mt_employee e
                INNER JOIN cf_role r ON e.RoleID = r.RoleID order by LastUpdate DESC");

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    userList.Add(new
                    {
                        UserName = Converter.TryToString(row["UserName"]),
                        ChineseName = Converter.TryToString(row["ChineseName"]),
                        RoleName = Converter.TryToString(row["RoleName"]),
                        RoleId = Converter.TryToInt32(row["RoleId"]),
                    });
                }
            }

            return userList;
        }

        internal static bool ResetPassword(string userName)
        {
            // 两次MD5加密
            string password = Config.DefaultPassword.ToMD5().ToMD5();
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    $"Update mt_employee set Password='{password}', WaitSet = 1 where UserName=@UserName;",
                new MySqlParameter("@UserName", userName));
            return true;
        }


        internal static bool SetPassword(string userName, string password)
        {
            password = password.ToUpper().ToMD5();
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    $"Update mt_employee set Password=@Password, WaitSet=0 where UserName=@UserName;",
                new MySqlParameter("@UserName", userName),
                new MySqlParameter("@Password", password));
            return true;
        }

        internal static bool SaveUser(string userName, string chineseName, int roleId)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    "Update mt_employee set ChineseName=@ChineseName,RoleID=@RoleID where UserName=@UserName;",
                new MySqlParameter("@UserName", userName),
                new MySqlParameter("@ChineseName", chineseName),
                new MySqlParameter("@RoleID", roleId));
            return true;
        }

        internal static bool DeleteUser(string userName)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    "Delete from mt_employee where UserName=@UserName;",
                new MySqlParameter("@UserName", userName));
            return true;
        }

        public static bool AddNewUser(string userName, string chineseName, int roleId)
        {
            object re = JabMySqlHelper.ExecuteScalar(Config.DBConnection,
                              "select count(*) from mt_employee where UserName=@UserName;",
                          new MySqlParameter("@UserName", userName));
            if (Converter.TryToInt32(re) > 0)
            {
                throw new MsgException("用户已存在");
            }
            string password = Config.DefaultPassword.ToMD5().ToMD5();

            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    $"INSERT INTO mt_employee (UserName,ChineseName,RoleID,Password) VALUES (@UserName,@ChineseName,@RoleID,'{password}');",
                new MySqlParameter("@UserName", userName),
                new MySqlParameter("@ChineseName", chineseName),
                new MySqlParameter("@RoleID", roleId));
            return true;
        }
    }
}
