using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressSystem.Api.Entity
{
    public class MyResult
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }


        public static MyResult OK(string msg = "")
        {
            return Result(ResultCode.OK, msg);
        }
        public static MyResult OK(object data)
        {
            return Result(ResultCode.OK, "", data);
        }
        public static MyResult OK(string msg, object data)
        {
            return Result(ResultCode.OK, msg, data);
        }
        public static MyResult Error(string msg = "")
        {
            return Result(ResultCode.ERROR, msg);
        }

        public static MyResult Result(ResultCode code, string msg, object data = null)
        {
            return new MyResult
            {
                Code = (int)code,
                Msg = msg,
                Data = data
            };
        }
    }

    public enum ResultCode
    {
        OK = 100,
        ERROR = 500
    }
}
