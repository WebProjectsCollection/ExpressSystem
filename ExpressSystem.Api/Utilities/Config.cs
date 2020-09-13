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
                string DBServer = Environment.GetEnvironmentVariable("DBServer") ?? ConfigurationManager.AppSettings["DBServer"];
                string DBName = Environment.GetEnvironmentVariable("DBName") ?? ConfigurationManager.AppSettings["DBName"];
                string DBUser = Environment.GetEnvironmentVariable("DBUser") ?? ConfigurationManager.AppSettings["DBUser"];
                string DBPwd = Environment.GetEnvironmentVariable("DBPwd") ?? ConfigurationManager.AppSettings["DBPwd"];
                string DBPort = Environment.GetEnvironmentVariable("DBPort") ?? ConfigurationManager.AppSettings["DBPort"];

                return $"Server={DBServer};Database={DBName};User ID={DBUser};Password={DBPwd};port={DBPort};pooling=true;Charset=utf8";

            }
        }

        public static string LDAP_API
        {
            get
            {
                return Environment.GetEnvironmentVariable("LDAP_API") ?? ConfigurationManager.AppSettings["LDAP_API"];
            }
        }
    }
}
