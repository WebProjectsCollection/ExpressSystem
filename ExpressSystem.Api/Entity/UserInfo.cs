using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.Api.Entity
{
    public class UserInfo
    {
        public long UserName { get; set; }
        public string Password { get; set; }
        public string ChineseName { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public int WaitSet { get; set; }
        public string Token { get; set; }
    }
}
