using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace ExpressSystem.Api.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpHelper
    {
        /// <summary>
        /// 简单的get请求
        /// </summary>
        public static string Get(string url, string data)
        {
            return Get(new HttpItem() { URL = url, Data = data }).Html;
        }

        /// <summary>
        /// 自定义request参数的get请求
        /// </summary>
        public static HttpResult Get(HttpItem item)
        {
            item.Method = "GET";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.URL + (item.Data == "" ? "" : "?") + item.Data);
            InitRequest(request, item);

            // 读取响应数据
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return GetResponseData(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                    {
                        return GetResponseData(response);
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 简单的form格式Post请求
        /// </summary>
        public static string Post(string url, string data)
        {
            return Post(new HttpItem() { URL = url, Data = data, ContentType = "application/x-www-form-urlencoded" }).Html;
        }

        /// <summary>
        /// 简单的json格式的Post请求
        /// </summary>
        public static string PostJson(string url, string data)
        {
            return Post(new HttpItem() { URL = url, Data = data, ContentType = "application/json" }).Html;
        }

        /// <summary>
        /// 自定义request参数的Post请求
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static HttpResult Post(HttpItem item)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.URL);
            item.Method = "POST";
            InitRequest(request, item);
            // 发送数据数据
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
            {
                writer.Write(item.Data);
                writer.Flush();
            }
            // 读取响应数据
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return GetResponseData(response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (HttpWebResponse response = (HttpWebResponse)ex.Response)
                    {
                        return GetResponseData(response);
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 从响应中获取cookies
        /// </summary>
        private static HttpResult GetResponseData(HttpWebResponse response)
        {
            HttpResult result = new HttpResult();
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
            {
                encoding = "UTF-8"; //默认编码 
            }
            // 读取响应数据
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding)))
            {
                result.Html = reader.ReadToEnd();
            }
            result.StatusCode = response.StatusCode;
            result.StatusDescription = response.StatusDescription;

            //获取CookieCollection
            if (response.Cookies != null)
            {
                result.CookieCollection = response.Cookies;
            }
            //获取set-cookie
            if (response.Headers["set-cookie"] != null)
            {
                result.Cookie = response.Headers["set-cookie"];
            }
            return result;
        }

        /// <summary>
        /// 初始化request对象
        /// </summary>
        private static void InitRequest(HttpWebRequest request, HttpItem item)
        {
            request.Accept = item.Accept;
            request.Method = item.Method;
            if (request.Method.ToLower() == "post")
            {
                request.ContentLength = item.Data.Length;
            }
            request.ContentType = item.ContentType;
            request.Timeout = item.Timeout;

            //设置Header参数
            if (item.Header != null && item.Header.Count > 0)
            {
                foreach (string key in item.Header.AllKeys)
                {
                    request.Headers.Add(key, item.Header[key]);
                }
            }
            //设置Cookie
            request.CookieContainer = new CookieContainer();
            if (item.CookieCollection != null && item.CookieCollection.Count > 0)
            {
                request.CookieContainer.Add(item.CookieCollection);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }
    }

    /// <summary>
    /// 响应数据
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        [JsonIgnore]
        public string Cookie { get; set; }

        /// <summary>
        /// Cookie对象集合
        /// </summary>
        [JsonIgnore]
        public CookieCollection CookieCollection { get; set; }

        /// <summary>
        /// 返回的String类型数据
        /// </summary>
        public string Html { get; set; } = string.Empty;

        /// <summary>
        /// 返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 响应时间
        /// </summary>
        public string ResponseTime { get; set; }
    }

    /// <summary>
    /// 请求对象
    /// </summary>
    public class HttpItem
    {
        /// <summary>
        /// 请求标头值 默认为text/html, application/xhtml+xml, */*
        /// </summary>
        [JsonIgnore]
        public string Accept { get; set; } = "text/json,*/*;q=0.5";
        /// <summary>
        /// 请求返回类型默认 text/html
        /// </summary>
        public string ContentType { get; set; } = "text/html;charset=UTF-8";

        /// <summary>
        /// Cookie对象集合
        /// </summary>
        [JsonIgnore]
        public CookieCollection CookieCollection { get; set; } = null;

        /// <summary>
        /// header对象
        /// </summary>
        [JsonIgnore]
        public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

        /// <summary>
        /// Post请求时要发送的字符串Post数据
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// 默认请求超时时间
        /// </summary>
        public int Timeout { get; set; } = 120000;

        /// <summary>
        /// 请求URL必须填写
        /// </summary>
        public string URL { get; set; } = string.Empty;

        /// <summary>
        /// 请求方式
        /// </summary>
        public string Method { get; set; } = "GET";
    }

    /// <summary>
    /// 请求方法
    /// </summary>
    public class RequestMethod
    {
        /// <summary>
        /// GET请求
        /// </summary>
        public const string GET = "GET";
        /// <summary>
        /// POST请求
        /// </summary>
        public const string POST = "POST";
    }

}
