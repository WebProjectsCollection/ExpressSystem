using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        // GET: api/Role
        [HttpGet("list")]
        public MyResult GetList(int siteId)
        {
            List<Object> roleList = RoleBLL.GetRoleList(siteId);
            return MyResult.OK(roleList);
        }

        // POST: api/Role
        [HttpPost("menu")]
        public MyResult PostMenu([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            int roleId = Convert.ToInt32(obj["roleId"]);
            JArray menuIds = JArray.FromObject(obj["menuIds"]);
            List<int> menuIdList = menuIds.ToObject<List<int>>();

            bool re = RoleBLL.SetPermission(roleId, menuIdList);
            return re ? MyResult.OK() : MyResult.Error();
        }

        // POST: api/Role
        [HttpGet("menus")]
        public MyResult GetMenus(int roleId)
        {
            List<int> menuIdLis = RoleBLL.GetRoleMenus(roleId);
            return MyResult.OK(menuIdLis);
        }
    }
}
