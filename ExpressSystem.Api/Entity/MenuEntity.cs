using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniformMSAPI.Entity
{
    public class MenuEntity
    {
        public int MenuID { get; set; }
        public string MenuText { get; set; }
        public string Icon { get; set; }
        public string RouterLink { get; set; }
        public int DisplayOrder { get; set; }
        public int ParentID { get; set; }
        public int ShowFlag { get; set; }
        public List<MenuEntity> Children { get; set; } = new List<MenuEntity>();
    }
}
