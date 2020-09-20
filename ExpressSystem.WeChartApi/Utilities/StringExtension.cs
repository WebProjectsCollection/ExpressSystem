// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   字符串拓展方法
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Security.Cryptography;

namespace ExpressSystem.WeChartApi.Utilities
{
    /// <summary>
    /// 字符串拓展方法
    /// </summary>
    public static class StringExtension
    {
        #region 是否和指定正则表达式完全匹配 +static bool IsSafeSql(this string input)
        /// <summary>
        /// 是否为安全SQL语句
        /// </summary>
        /// <param name="input">指定字符串</param>
        /// <returns>真或假</returns>
        public static bool IsMatch(this string input, string reg)
        {
            return !Regex.IsMatch(input, reg);
        }
        #endregion

        #region 是否为安全SQL语句 +static bool IsSafeSql(this string input)
        /// <summary>
        /// 是否为安全SQL语句
        /// </summary>
        /// <param name="input">指定字符串</param>
        /// <returns>真或假</returns>
        public static bool IsSafeSql(this string input)
        {
            return !Regex.IsMatch(input, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }
        #endregion

        #region 判断指定字符串是否为合法IP地址 +static bool IsIpAddress(this string input)
        /// <summary>
        /// 判断指定字符串是否为合法IP地址
        /// </summary>
        /// <param name="input">指定字符串</param>
        /// <returns>真或假</returns>
        public static bool IsIpAddress(this string input)
        {
            return Regex.IsMatch(input, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
        #endregion

        #region 判断指定字符串是否合法的日期格式 +static bool IsDateTime(this string input)
        /// <summary>
        /// 判断指定字符串是否合法的日期格式
        /// </summary>
        /// <param name="input">指定字符串</param>
        /// <returns>真或假</returns>
        public static bool IsDateTime(this string input)
        {
            DateTime dt;
            return DateTime.TryParse(input, out dt);
        }
        #endregion

        #region 判断指定的字符串是否为数字 +static bool IsInt(this string str)
        /// <summary>
        /// 判断指定的字符串是否为数字
        /// </summary>
        /// <param name="str">要确认的字符串</param>
        /// <returns>真或假</returns>
        public static bool IsInt(this string str)
        {
            return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, "^-?\\d+$");
        }
        #endregion

        #region 判断指定的字符串是否为小数 +static bool IsFloat(this string str)
        /// <summary>
        /// 判断指定的字符串是否为小数
        /// </summary>
        /// <param name="str">要确认的字符串</param>
        /// <returns>真或假</returns>
        public static bool IsFloat(this string str)
        {
            return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, @"^-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0)$");
        }
        #endregion

        #region 判断指定的字符串是否为Url地址 +static bool IsUrl(this string str)
        /// <summary>
        /// 判断指定的字符串是否为Url地址
        /// </summary>
        /// <param name="str">要确认的字符串</param>
        /// <returns>真或假</returns>
        public static bool IsUrl(this string str)
        {
            return Regex.IsMatch(str, "(http[s]{0,1}|ftp)://[a-zA-Z0-9\\.\\-]+\\.([a-zA-Z]{2,4})(:\\d+)?(/[a-zA-Z0-9\\.\\-~!@#$%^&*+?:_/=<>]*)?");
        }
        #endregion

        #region 判断指定的字符串是否为合法Email +static bool IsEmail(this string str)
        /// <summary>
        /// 判断指定的字符串是否为合法Email
        /// </summary>
        /// <param name="str">指定的字符串</param>
        /// <returns>真或假</returns>
        public static bool IsEmail(this string str)
        {
            return Regex.IsMatch(str, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
        #endregion

        #region 判断字符串是否为Null或者为空 +static bool IsNullOrEmpty(this string input)
        /// <summary>
        /// 判断字符串是否为Null或者为空
        /// </summary>
        /// <param name="input">要判断的字符串</param>
        /// <returns>是否为Null或者为空</returns>
        public static bool IsNullOrEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }
        #endregion

        #region 判断字符串是否为Null或者为空字符组成 +static bool IsNullOrWhiteSpace(this string input)
        /// <summary>
        /// 判断字符串是否为Null或者为空字符组成
        /// </summary>
        /// <param name="input">要判断的字符串</param>
        /// <returns>否为Null或者为空字符组成</returns>
        public static bool IsNullOrWhiteSpace(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }
        #endregion


        #region 对字符串进行 HTML 编码并返回已编码的字符串 +static string HtmlEncode(this string content)
        /// <summary>
        /// 对字符串进行 HTML 编码并返回已编码的字符串
        /// </summary>
        /// <param name="content">要编码的文本字符串</param>
        /// <returns>HTML 已编码的文本</returns>
        public static string HtmlEncode(this string content)
        {
            return HttpUtility.HtmlEncode(content);
        }
        #endregion

        #region 对 HTML 编码的字符串进行解码，并返回已解码的字符串 +static string HtmlDecode(this string content)
        /// <summary>
        /// 对 HTML 编码的字符串进行解码，并返回已解码的字符串
        /// </summary>
        /// <param name="content">要解码的文本字符串</param>
        /// <returns>已解码的字符串</returns>
        public static string HtmlDecode(this string content)
        {
            return HttpUtility.HtmlDecode(content);
        }
        #endregion

        #region 对 URL 字符串进行编码, 返回 URL 字符串的编码结果 +static string UrlEncode(this string str, Encoding e = null)
        /// <summary>
        /// 对 URL 字符串进行编码, 返回 URL 字符串的编码结果
        /// </summary>
        /// <param name="str">要编码的文本</param>
        /// <param name="e">编码</param>
        /// <returns>一个已编码的字符串</returns>
        public static string UrlEncode(this string str, Encoding e = null)
        {
            e = e ?? Encoding.UTF8;
            return HttpUtility.UrlEncode(str, e);
        }
        #endregion

        #region 对 URL 字符串进行解码, 返回 URL 字符串的解码结果 +static string UrlDecode(this string str, Encoding e = null)
        /// <summary>
        /// 对 URL 字符串进行解码, 返回 URL 字符串的解码结果
        /// </summary>
        /// <param name="str">要解码的文本</param>
        /// <param name="e">编码</param>
        /// <returns>解码结果</returns>
        public static string UrlDecode(this string str, Encoding e = null)
        {
            e = e ?? Encoding.UTF8;
            return HttpUtility.UrlDecode(str);
        }
        #endregion

        #region 移除Html标记 +static string TrimHtml(this string html)
        /// <summary>
        /// 移除Html标记
        /// </summary>
        /// <param name="html">包括HTML的源码</param>
        /// <returns>已经去除后的文字</returns>
        public static string TrimHtml(this string html)
        {
            // 删除脚本和嵌入式CSS   
            html = Regex.Replace(html, @"<script[^>]*?>.*?</script>", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[^>]*?>.*?</style>", string.Empty, RegexOptions.IgnoreCase);

            // 删除HTML   
            var regex = new Regex("<.+?>", RegexOptions.IgnoreCase);
            html = regex.Replace(html, string.Empty);
            html = Regex.Replace(html, @"<(.[^>]*)>", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"([\r\n])[\s]+", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"-->", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<!--.*", string.Empty, RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&#(\d+);", string.Empty, RegexOptions.IgnoreCase);

            return html.Replace("<", string.Empty).Replace(">", string.Empty).Replace("\r\n", string.Empty);
        }
        #endregion

        #region 返回字符串真实长度, 1个汉字长度为2
        /// <summary>
        /// 返回字符串真实长度, 1个汉字长度为2
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>System.Int32.</returns>
        public static int Length(this string str)
        {
            return string.IsNullOrEmpty(str) ? 0 : Encoding.UTF8.GetBytes(str).Length;
        }
        #endregion


        #region 字符串风格转换
        /// <summary>
        /// The to pascal case.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToPascalCase(this string input)
        {
            // If there are 0 or 1 characters, just return the string.
            if (input == null)
            {
                return null;
            }

            if (input.Length < 2)
            {
                return input.ToUpper();
            }

            // Split the string into words.
            string[] words = input.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            var result = new StringBuilder();
            foreach (string word in words)
            {
                result.Append(word.Substring(0, 1).ToUpper()).Append(word.Substring(1));
            }

            return result.ToString();
        }

        /// <summary>
        /// The to camel case.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToCamelCase(this string input)
        {
            // If there are 0 or 1 characters, just return the string.
            if (input == null)
            {
                return null;
            }

            if (input.Length < 2)
            {
                return input.ToLower();
            }

            // Split the string into words.
            string[] words = input.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            var result = new StringBuilder(words[0].ToLower());
            for (int i = 1; i < words.Length; i++)
            {
                result.Append(words[i].Substring(0, 1).ToUpper()).Append(words[i].Substring(1));

            }

            return result.ToString();
        }

        /// <summary>
        /// The pascal case word boundary regex.
        /// </summary>
        static readonly Regex PascalCaseWordBoundaryRegex = new Regex(@"
(?# word to word, number or acronym)
(?<=[a-z])(?=[A-Z0-9])|
(?# number to word or acronym)
(?<=[0-9])(?=[A-Za-z])|
(?# acronym to number)
(?<=[A-Z])(?=[0-9])|
(?# acronym to word)
(?<=[A-Z])(?=[A-Z][a-z])
", RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// The to underline case.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToUnderlineCase(this string input)
        {
            var result = PascalCaseWordBoundaryRegex
                .Split(input)
                .Select(word =>
                    word.ToCharArray().All(char.IsUpper) && word.Length > 1
                        ? word
                        : word.ToLower())
                .Aggregate((res, word) => res + " " + word);

            // result = Char.ToUpper(result[0]) + result.Substring(1, result.Length - 1);
            // return result.ToLower();//.Replace(" i ", " I "); // I is an exception
            // if (input == null) return input;
            // if (input.Length < 2) return input.ToLower();

            //// Split the string into words.
            var words = result.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.ToLower());
            return string.Join("_", words).ToLower();
        }

        /// <summary>
        /// 转换成正常格式
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToProperCase(this string input)
        {
            // If there are 0 or 1 characters, just return the string.
            if (input == null)
            {
                return null;
            }

            if (input.Length < 2)
            {
                return input.ToUpper();
            }

            // Start with the first character.
            var result = new StringBuilder(input.Substring(0, 1).ToUpper());

            // Add the remaining characters.
            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    result.Append(" ");
                }

                result.Append(input[i]);
            }

            return result.ToString();
        }
        #endregion

        #region 将指定字符串中的格式项替换为指定数组中相应对象的字符串表示形式。
        /// <summary>
        /// 将指定字符串中的格式项替换为指定数组中相应对象的字符串表示形式。
        /// </summary>
        /// <param name="input">复合格式字符串。</param>
        /// <param name="args">一个对象数组，其中包含零个或多个要设置格式的对象。</param>
        /// <returns>format 的副本，其中的格式项已替换为 args 中相应对象的字符串表示形式。</returns>
        /// <exception cref="System.ArgumentNullException">input 或 args 为 null。</exception>
        /// <exception cref="System.FormatException">format 无效。- 或 -格式项的索引小于零或大于等于 args 数组的长度。</exception>
        public static string Format(this string input, params object[] args)
        {
            return string.Format(input, args);
        }
        #endregion

        #region Convert string type to other types

        #region String to Enum +static T ToEnum<T>(this string s) where T : struct
        /// <summary>
        /// String to Enum<see cref="T"/>
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string s) where T : struct
        {
            T result;
            Enum.TryParse(s, true, out result);
            return result;
        }
        #endregion

        #region String to Slug +static string ToSlug(this string input)
        /// <summary>
        /// String to Slug
        /// </summary>
        /// <param name="input">String</param>
        /// <returns>Slug</returns>
        public static string ToSlug(this string input)
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty : Regex.Replace(input.Trim().ToLower(), @"\s+", "-");
        }
        #endregion

        /// <summary>
        /// 转换成Key的形式
        /// </summary>
        /// <param name="input">输入</param>
        /// <returns>输出</returns>
        public static string ToKeyCase(this string input)
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty : Regex.Replace(input.Trim().ToLower(), @"\s+", "_");
        }

        #endregion


        #region 将一个空字符串替换为指定的值 +static string IfEmptyToString(this string input, string val = "")
        /// <summary>
        /// 将一个空字符串替换为指定的值
        /// </summary>
        /// <param name="input">要转化的字符串</param>
        /// <param name="val">指定的值</param>
        /// <returns></returns>
        public static string IfEmptyToString(this string input, string val = "")
        {
            return string.IsNullOrEmpty(input) ? val : input;
        }

        #endregion

        /// <summary>
        /// MD5加密字符串（32位大写）
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string ToMD5(this string input)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string result = BitConverter.ToString(md5.ComputeHash(bytes));
            return result.Replace("-", "").ToUpper();
        }
    }
}
