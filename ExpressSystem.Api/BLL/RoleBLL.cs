using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.BLL
{
    public class RoleBLL
    {
        public static List<Object> GetRoleList()
        {
            List<Object> roleList = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                "select * from cf_role");

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    roleList.Add(new
                    {
                        RoleID = Converter.TryToInt32(row["RoleID"]),
                        RoleName = Converter.TryToString(row["RoleName"])
                    });
                }
            }

            return roleList;
        }

        public static bool SetPermission(int roleId, List<int> menuIds)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                "Delete from cf_rolepermission where RoleID=@RoleID",
                new MySqlParameter("@RoleID", roleId));

            foreach (int menuId in menuIds)
            {
                JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    "INSERT INTO cf_rolepermission (RoleID, MenuID) VALUES (@RoleID, @MenuID);",
                new MySqlParameter("@RoleID", roleId),
                new MySqlParameter("@MenuID", menuId));
            }
            return true;
        }

        internal static List<int> GetRoleMenus(int roleId)
        {
            List<int> menuList = new List<int>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                "select menuId from cf_rolepermission where RoleID=@RoleID",
                new MySqlParameter("@RoleID", roleId));

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    menuList.Add(Converter.TryToInt32(row["menuId"]));
                }
            }

            return menuList;
        }
    }
}
