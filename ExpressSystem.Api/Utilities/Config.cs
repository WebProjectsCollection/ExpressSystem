using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ExpressSystem.Api.Utilities
{
    public class Config
    {
        public static string DBConnection
        {
            get
            {
                string DBServer = ConfigurationManager.AppSettings["DBServer"];
                string DBName = ConfigurationManager.AppSettings["DBName"];
                string DBUser = ConfigurationManager.AppSettings["DBUser"];
                string DBPwd = ConfigurationManager.AppSettings["DBPwd"];
                string DBPort = ConfigurationManager.AppSettings["DBPort"];

                return $"Server={DBServer};Database={DBName};User ID={DBUser};Password={DBPwd};port={DBPort};pooling=true;Charset=utf8";

            }
        }
        public static string DefaultPassword
        {
            get { return "123456"; }
        }
    }
}
