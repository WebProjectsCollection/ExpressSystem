using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.BLL
{
    public class DefaultSettingBLL
    {
        public static bool SaveData(int siteId, string uniformType, int applyNumber, string hrEmails)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                            "Delete from cf_defaultsetting where SiteID=@SiteID",
                            new MySqlParameter("@SiteID", siteId));

            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                @"INSERT INTO cf_defaultsetting (SiteID, UniformType, ApplyNumber, HREmails)
                  VALUES (@SiteID, @UniformType, @ApplyNumber, @HREmails);",
            new MySqlParameter("@SiteID", siteId),
            new MySqlParameter("@UniformType", uniformType),
            new MySqlParameter("@ApplyNumber", applyNumber),
            new MySqlParameter("@HREmails", hrEmails));

            return true;
        }

        public static object GetData(int siteId)
        {
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                "select * from cf_defaultsetting where siteId=@siteId",
                new MySqlParameter("@siteId", siteId));

            object data = null;
            if (dt != null && dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                data = new
                {
                    SiteID = Converter.TryToInt32(row["SiteID"]),
                    UniformType = Converter.TryToString(row["UniformType"]),
                    ApplyNumber = Converter.TryToInt32(row["ApplyNumber"]),
                    HREmails = Converter.TryToString(row["HREmails"])
                };
            }
            return data;
        }
    }
}
