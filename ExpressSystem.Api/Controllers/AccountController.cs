using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // POST: api/Login
        [HttpPost("login")]
        public MyResult Post([FromBody] LoginInfo user)
        {

            // 读取用户信息
            UserInfo userInfo = UserBLL.GetUserDetail(user.UserName, user.SiteId);

            if (userInfo == null)
            {
                throw new MsgException("用户未注册");
            }
            // 缓存一天
            userInfo.Token = JObject.FromObject(user).ToString().ToMD5();
            CacheHelper.CacheInsertAddMinutes(userInfo.Token, user, 24 * 60);
            return MyResult.OK(userInfo);
        }

        [HttpGet("logout")]
        public MyResult Get(string token)
        {
            CacheHelper.CacheNull(token);
            return MyResult.OK();
        }

        [HttpGet("checkToken")]
        public MyResult CheckToken(string token)
        {
            object value = CacheHelper.CacheValue(token);
            return value == null ? MyResult.Error() : MyResult.OK();
        }
    }
}
