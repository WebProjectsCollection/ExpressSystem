using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.WeChartApi.Entity
{
    public class StatusInfo
    {
        public string Order_Num { get; set; }
        public string Update_Date { get; set; }
        public string Update_Time { get; set; }
        public string Update_Status { get; set; }
        public string Remarks { get; set; }

        //扩展
        public bool LastFlag { get; set; }
    }
}
