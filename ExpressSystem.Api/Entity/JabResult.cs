using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniformMSAPI.Entity
{
    public class JabResult
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }


        public static JabResult OK(string msg = "")
        {
            return Result(ResultCode.OK, msg);
        }
        public static JabResult OK(object data)
        {
            return Result(ResultCode.OK, "", data);
        }
        public static JabResult OK(string msg, object data)
        {
            return Result(ResultCode.OK, msg, data);
        }
        public static JabResult Error(string msg = "")
        {
            return Result(ResultCode.ERROR, msg);
        }

        public static JabResult Result(ResultCode code, string msg, object data = null)
        {
            return new JabResult
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
