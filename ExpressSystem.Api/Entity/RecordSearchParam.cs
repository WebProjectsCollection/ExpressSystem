using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.Api.Entity
{
    public class RecordSearchParam
    {
        public int SiteId { get; set; }
        public string OutTimeStartStr { get; set; }
        public string OutTimeEndStr { get; set; }
        public string BackTimeStartStr { get; set; }
        public string BackTimeEndStr { get; set; }
        public string EmployeeId { get; set; }
        public string Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
