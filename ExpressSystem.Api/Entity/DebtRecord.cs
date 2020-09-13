using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.Api.Entity
{
    public class DebtRecord
    {
        public string ChineseName { get; set; }
        public long EmployeeID { get; set; }
        public string Department { get; set; }
        public string Costcenter { get; set; }
        public decimal Debt { get; set; }
        public string Details { get; set; }
        public string InDate { get; set; }
        public string LeaveDate { get; set; }
    }
}
