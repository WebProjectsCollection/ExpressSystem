using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniformMSAPI.Entity
{
    public class UserInfo
    {
        public long EmployeeID { get; set; }
        public string NTID { get; set; }
        public string ChineseName { get; set; }
        public string EmailAddress { get; set; }
        public string Department { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public int SiteId { get; set; }
        public string Token { get; set; }
    }
}
