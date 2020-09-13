using System;
using System.Collections.Generic;
using System.Data;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.BLL
{
    public class SiteBLL
    {
        public static List<Object> GetSiteList()
        {
            List<Object> siteList = new List<Object>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, "select * from cf_site", null);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    siteList.Add(new
                    {
                        SiteID = Converter.TryToInt32(row["SiteID"]),
                        SiteName = Converter.TryToString(row["SiteName"])
                    }); ;
                }
            }

            return siteList;
        }
    }
}
