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
    public class UniformTypeBLL
    {
        public static List<UniformType> getAllBySite(int siteId)
        {
            List<UniformType> uniformList = new List<UniformType>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                "select * from cf_uniformtype where siteId=@siteId",
                new MySqlParameter("@siteId", siteId));

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    uniformList.Add(new UniformType
                    {
                        Season = Converter.TryToString(row["Season"]),
                        Style = Converter.TryToString(row["Style"]),
                        SiteID = Converter.TryToInt32(row["SiteID"]),
                        Price = Converter.TryToDecimal(row["Price"]),
                        Stock = Converter.TryToInt32(row["Stock"]),
                    }); ;
                }
            }
            return uniformList;
        }

        internal static List<string> getAllSessions(int siteId)
        {
            List<string> list = new List<string>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection,
                "select distinct Season from cf_uniformtype where siteId=@siteId",
                new MySqlParameter("@siteId", siteId));

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(Converter.TryToString(row["Season"])); ;
                }
            }
            return list;
        }

        public static bool SaveData(int siteId, List<UniformType> list)
        {
            JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                "Delete from cf_uniformtype where SiteID=@SiteID",
                new MySqlParameter("@SiteID", siteId));

            // 去重
            list = list.GroupBy(x => x.Style).Select(y => y.FirstOrDefault()).ToList();

            foreach (UniformType item in list)
            {
                JabMySqlHelper.ExecuteNonQuery(Config.DBConnection,
                    "INSERT INTO cf_uniformtype (Season, Style, SiteID, Price, Stock) VALUES (@Season, @Style, @SiteID, @Price, @Stock);",
                new MySqlParameter("@Season", item.Season),
                new MySqlParameter("@Style", item.Style),
                new MySqlParameter("@SiteID", siteId),
                new MySqlParameter("@Price", item.Price),
                new MySqlParameter("@Stock", item.Stock));
            }
            return true;
        }
    }
}
