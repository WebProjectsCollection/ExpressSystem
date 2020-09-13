using System.Collections.Generic;
using System.Data;
using System.Linq;
using ExpressSystem.Api.Entity;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.BLL
{
    public static class MenuBLL
    {
        public static List<MenuEntity> GetMenuList()
        {
            List<MenuEntity> menuList = new List<MenuEntity>();
            DataTable dt = JabMySqlHelper.ExecuteDataTable(Config.DBConnection, "select * from cf_menus where ShowFlag=1 order by DisplayOrder ", null);

            if (dt != null && dt.Rows.Count > 0)
            {
                List<MenuEntity> tempList = new List<MenuEntity>();
                foreach (DataRow row in dt.Rows)
                {
                    tempList.Add(new MenuEntity()
                    {
                        MenuID = Converter.TryToInt32(row["MenuID"]),
                        MenuText = Converter.TryToString(row["MenuText"]),
                        Icon = Converter.TryToString(row["Icon"]),
                        RouterLink = Converter.TryToString(row["RouterLink"]),
                        ParentID = Converter.TryToInt32(row["ParentID"]),
                    }); ;
                }
                menuList = tempList.Where(m => m.ParentID == -1).ToList();
                foreach (MenuEntity item in menuList)
                {
                    item.Children = tempList.Where(m => m.ParentID == item.MenuID).ToList();
                }
            }

            return menuList;
        }
    }
}
