using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    public class MenuController : Controller
    {
        // GET: api/<controller>
        [HttpGet]
        public MyResult Get()
        {
            List<MenuEntity> menuList = MenuBLL.GetMenuList();
            return MyResult.OK(menuList);
        }
    }
}
