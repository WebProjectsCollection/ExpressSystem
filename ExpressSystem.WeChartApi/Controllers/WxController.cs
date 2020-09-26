using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using ExpressSystem.WeChartApi.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpressSystem.WeChartApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WxController : ControllerBase
    {
        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [NonAction]
        private string MakeSign(params string[] args)
        {
            //字典排序
            Array.Sort(args);
            string tmpStr = string.Join("", args);
            //字符加密
            var sha1 = EncryptHelper.Sha1Encrypt(tmpStr);
            return sha1;
        }
        /// <summary>
        /// 生成消息签名
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [NonAction]
        private string MakeMsgSign(params string[] args)
        {
            //字典排序
            Array.Sort(args, new CharSort());
            string tmpStr = string.Join("", args);
            //字符加密
            var sha1 = EncryptHelper.Sha1Encrypt(tmpStr);
            return sha1;
        }
        /// <summary>
        /// 微信回调统一接口
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public string Service()
        {
            //获取配置文件中的数据
            var token = "";
            var encodingAESKey = "";
            var appId = "";

            bool isGet = string.Equals(HttpContext.Request.Method, HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase);
            bool isPost = string.Equals(HttpContext.Request.Method, HttpMethod.Post.Method, StringComparison.OrdinalIgnoreCase);
            if (!isGet && !isPost)
            {
                return "";
            }

            bool isEncrypt = false;
            try
            {
                var query = HttpContext.Request.QueryString.ToString();
                string msg_signature = "", nonce = "", timestamp = "", encrypt_type = "", signature = "", echostr = "";

                if (!string.IsNullOrEmpty(query))//需要验证签名
                {
                    var collection = HttpUtility.ParseQueryString(query);
                    msg_signature = collection["msg_signature"]?.Trim();
                    nonce = collection["nonce"]?.Trim();
                    timestamp = collection["timestamp"]?.Trim();
                    encrypt_type = collection["encrypt_type"]?.Trim();
                    signature = collection["signature"]?.Trim();
                    echostr = collection["echostr"]?.Trim();

                    if (!string.IsNullOrEmpty(encrypt_type))//有使用加密
                    {
                        if (!string.Equals(encrypt_type, "aes", StringComparison.OrdinalIgnoreCase))//只支持AES加密方式
                        {
                            return "";
                        }
                        isEncrypt = true;
                    }
                }

                //先验证签名
                if (!string.IsNullOrEmpty(signature))
                {
                    //字符加密
                    var sha1 = MakeSign(nonce, timestamp, token);
                    if (!sha1.Equals(signature, StringComparison.OrdinalIgnoreCase))//验证不通过
                    {
                        return "";
                    }

                    if (isGet)//是否Get请求，如果true,那么就认为是修改服务器回调配置信息
                    {
                        return echostr;
                    }
                }
                else
                {
                    return "";//没有签名，请求直接返回
                }

                var body = new StreamReader(HttpContext.Request.Body).ReadToEnd();

                if (isEncrypt)
                {
                    XDocument doc = XDocument.Parse(body);
                    var encrypt = doc.Element("xml").Element("Encrypt");

                    //验证消息签名
                    if (!string.IsNullOrEmpty(msg_signature))
                    {
                        //消息加密
                        var sha1 = MakeMsgSign(nonce, timestamp, encrypt.Value, token);
                        if (!sha1.Equals(msg_signature, StringComparison.OrdinalIgnoreCase))//验证不通过
                        {
                            return "";
                        }
                    }

                    body = EncryptHelper.AESDecrypt(encrypt.Value, encodingAESKey);//解密
                }

                if (!string.IsNullOrEmpty(body))
                {
                    //
                    //在这里根据body中的MsgType和Even来区分消息，然后来处理不同的业务逻辑
                    //
                    //

                    //result是上面逻辑处理完成之后的待返回结果，如返回文本消息：
                    var result = @"<xml>
                                      <ToUserName><![CDATA[toUser]]></ToUserName>
                                      <FromUserName><![CDATA[fromUser]]></FromUserName>
                                      <CreateTime>12345678</CreateTime>
                                      <MsgType><![CDATA[text]]></MsgType>
                                      <Content><![CDATA[你好]]></Content>
                                    </xml>";
                    if (!string.IsNullOrEmpty(result))
                    {
                        if (isEncrypt)
                        {
                            result = EncryptHelper.AESEncrypt(result, encodingAESKey, appId);
                            var _msg_signature = MakeMsgSign(nonce, timestamp, result, token);
                            result = $@"<xml>
                                                    <Encrypt><![CDATA[{result}]]></Encrypt>
                                                    <MsgSignature>{_msg_signature}</MsgSignature>
                                                    <TimeStamp>{timestamp}</TimeStamp>
                                                    <Nonce>{nonce}</Nonce>
                                                </xml>";
                        }
                        return result;
                    }

                    //如果这里我们的处理逻辑需要花费较长时间，可以这里先返回空(""),然后使用异步去处理业务逻辑，
                    //异步处理完后，调用微信的客服消息接口通知微信服务器
                }
            }
            catch (Exception ex)
            {
                //记录异常日志
            }

            return "";
        }
    }
}