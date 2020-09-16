using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ExpressSystem.Api.BLL;
using ExpressSystem.Api.Entity;
using ExpressSystem.Api.Utilities;

namespace ExpressSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // GET: api/User
        [HttpGet]
        public MyResult GetList()
        {
            List<Object> roleList = UserBLL.GetUserList();
            return MyResult.OK(roleList);
        }

        // POST: api/User
        [HttpPost]
        public MyResult Post([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            string userName = Convert.ToString(obj["userName"]);
            string chineseName = Convert.ToString(obj["chineseName"]);
            int roleId = Convert.ToInt32(obj["roleId"]);
            bool isNew = Convert.ToBoolean(obj["isNew"]);

            if (string.IsNullOrWhiteSpace(userName))
            {
                return MyResult.Error("用户名不能为空！");
            }
            if (string.IsNullOrWhiteSpace(chineseName))
            {
                return MyResult.Error("姓名不能为空！");
            }

            bool re = isNew ? UserBLL.AddNewUser(userName, chineseName, roleId) : UserBLL.SaveUser(userName, chineseName, roleId);
            return re ? MyResult.OK() : MyResult.Error();
        }

        [HttpPost("resetPassword")]
        public MyResult ResetPassword([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            string userName = Convert.ToString(obj["userName"]);

            if (string.IsNullOrWhiteSpace(userName))
            {
                return MyResult.Error("用户名不能为空！");
            }

            bool re = UserBLL.ResetPassword(userName);
            return re ? MyResult.OK() : MyResult.Error();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{userName}")]
        public MyResult Delete(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return MyResult.Error("用户名不能为空！");
            }

            bool re = UserBLL.DeleteUser(userName);
            return re ? MyResult.OK() : MyResult.Error();
        }

        [Produces("application/json")]
        [Consumes("application/json", "multipart/form-data")]
        [HttpPost("importEmployee")]
        public IActionResult importEmployee(IFormFile file)
        {
            try
            {
                DataTable dt = ExcelHelper.ReadExcelToDataTable(file.OpenReadStream(), "Sheet1");

                MySqlHelper.ExecuteNonQuery(Config.DBConnection, "TRUNCATE TABLE tmp_importemployee");

                string sql = @"INSERT INTO tmp_importemployee
                                (`EmployeeID`,
                                `ChineseName`,
                                `EmploymentStatus`,
                                `EmailAddress`,
                                `SupervisorID`,
                                `Department`,
                                `Costcenter`,
                                `BadgeID`,
                                `AreaCode`) VALUES ";
                List<string> values = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    string chineseName = Convert.ToString(row[0]);
                    long employeeID = Convert.ToInt64(row[1]);
                    string badgeID = Convert.ToString(row[2]);
                    string costcenter = Convert.ToString(row[4]);
                    string employmentStatus = "是";
                    try
                    {
                        employmentStatus = Convert.ToString(row["是否在职"]);
                    }
                    catch (Exception) { }
                    string emailAddress = "";
                    try
                    {
                        emailAddress = Convert.ToString(row["邮箱"]);
                    }
                    catch (Exception) { }
                    long supervisorID = -1;
                    try
                    {
                        supervisorID = Converter.TryToInt64(row["主管工号"], -1);
                    }
                    catch (Exception) { }
                    string department = "";
                    try
                    {
                        department = Convert.ToString(row["部门"]);
                    }
                    catch (Exception) { }
                    string areaCode = "CN51";
                    try
                    {
                        areaCode = Convert.ToString(row["AreaCode"]);
                    }
                    catch (Exception) { }

                    employmentStatus = employmentStatus == "是" ? "1" : "0";

                    values.Add($"({employeeID},'{chineseName}',{employmentStatus},'{emailAddress}','{supervisorID}','{department}','{costcenter}','{badgeID}','{areaCode}')");
                }
                if (values.Count == 0)
                {
                    throw new Exception("导入人员信息为空！");
                }
                MySqlHelper.ExecuteNonQuery(Config.DBConnection, sql + string.Join(",", values));

                MySqlHelper.ExecuteNonQuery(Config.DBConnection, "call Sp_confirmEmployee()");

                return Ok(new
                {
                    status = 200,
                    msg = "导入成功"
                });
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    status = 500,
                    msg = "导入失败:" + e.Message,
                    error = "导入失败:" + e.Message + Environment.NewLine + e.StackTrace
                });
            }
        }
    }
}
