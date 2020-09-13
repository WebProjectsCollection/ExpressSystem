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
        public MyResult GetList(int siteId)
        {
            List<Object> roleList = UserBLL.GetUserList(siteId);
            return MyResult.OK(roleList);
        }

        // GET: api/User/5
        [HttpGet("info")]
        public MyResult GetInfo(string badgeId = "", string employeeId = "")
        {
            object userObj = UserBLL.GetUserInfoById(badgeId, employeeId);
            return userObj == null ? MyResult.Error("未找到员工信息") : MyResult.OK(userObj);
        }

        [HttpGet("stockInfo")]
        public MyResult GetStockInfo(string badgeId = "")
        {
            object userObj = UserBLL.GetUserStockInfo(badgeId);
            return userObj == null ? MyResult.Error("未找到员工信息") : MyResult.OK(userObj);
        }

        [HttpGet("getByRole")]
        public MyResult GetByRole(string employeeId, int siteId, string roleCode)
        {
            object userObj = UserBLL.GetUserByRole(employeeId, siteId, roleCode);
            return MyResult.OK(userObj);

        }


        [HttpGet("applyRecords")]
        public MyResult GetapplyRecords(long employeeId)
        {
            object list = UserBLL.GetapplyRecords(employeeId);
            return MyResult.OK(list);
        }

        // POST: api/User
        [HttpPost]
        public MyResult Post([FromBody] object data)
        {
            JObject obj = JObject.FromObject(data);
            int roleId = Convert.ToInt32(obj["roleId"]);
            string ntid = Convert.ToString(obj["ntid"]);
            string employeeId = Convert.ToString(obj["employeeId"]);
            bool isNew = Convert.ToBoolean(obj["isNew"]);

            bool re = isNew ? UserBLL.AddNewUser(ntid, employeeId, roleId) : UserBLL.SaveUser(ntid, employeeId, roleId);
            return re ? MyResult.OK() : MyResult.Error();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{ntid}")]
        public MyResult Delete(string ntid)
        {
            bool re = UserBLL.DeleteUser(ntid);
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
