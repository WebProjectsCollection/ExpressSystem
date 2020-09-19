using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExpressSystem.WeChartApi.Utilities
{
    /// <summary>
    /// 转换拓展方法
    /// </summary>
    public static partial class Converter
    {
        #region Convert DateTime type to other types

        private const string DateFormat = "yyyy-MM-dd HH:ss";
        /// <summary>
        /// 时间转换成字符串
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <returns>字符串</returns>
        public static string ToString(this DateTime dateTime)
        {
            return dateTime.ToString(DateFormat);
        }

        /// <summary>
        /// 将DateTime时间换成中文
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <returns>System.String.</returns>
        /// <example>
        /// 2012-12-21 12:12:21.012 → 1月前
        /// 2011-12-21 12:12:21.012 → 1年前
        /// </example>
        public static string ToChsStr(this DateTime dateTime)
        {
            var ts = DateTime.Now - dateTime;
            if ((int)ts.TotalDays >= 365)
                return (int)ts.TotalDays / 365 + "年前";
            if ((int)ts.TotalDays >= 30 && ts.TotalDays <= 365)
                return (int)ts.TotalDays / 30 + "月前";
            if ((int)ts.TotalDays == 1)
                return "昨天";
            if ((int)ts.TotalDays == 2)
                return "前天";
            if ((int)ts.TotalDays >= 3 && ts.TotalDays <= 30)
                return (int)ts.TotalDays + "天前";
            if ((int)ts.TotalDays != 0) return dateTime.ToString("yyyy年MM月dd日");
            if ((int)ts.TotalHours != 0)
                return (int)ts.TotalHours + "小时前";
            if ((int)ts.TotalMinutes == 0)
                return "刚刚";
            return (int)ts.TotalMinutes + "分钟前";
        }

        #endregion

        /// <summary>
        /// 将DataTable转化为对应的实体列表
        /// </summary>
        /// <typeparam name="T">T is T</typeparam>
        /// <param name="dt">DataTable数据源</param>
        /// <returns>实体列表</returns>
        public static List<T> DataTableToList<T>(DataTable dt) where T : class
        {
            List<T> modelList = new List<T>();
            if (dt != null)
            {
                int rowsCount = dt.Rows.Count;
                if (rowsCount > 0)
                {
                    // 创建实体
                    T model = Activator.CreateInstance<T>();
                    for (int n = 0; n < rowsCount; n++)
                    {
                        model = DataRowToModel<T>(dt.Rows[n]);
                        if (model != null)
                        {
                            modelList.Add(model);
                        }
                    }
                }
            }
            return modelList;
        }

        /// <summary>
        /// 将DataRow转化为指定类型实体
        /// </summary>
        /// <typeparam name="T">T is T</typeparam>
        /// <param name="row">DataRow数据源</param>
        /// <returns>转换所得实体</returns>
        public static T DataRowToModel<T>(DataRow row) where T : class
        {
            // 创建实体
            T result = Activator.CreateInstance<T>();
            if (row == null)
            {
                return result;
            }

            // 获取该类型的所有属性
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                // 遍历所有属性，并在DataRow中获取对应的值
                object value = row[prop.Name];

                if (value != null && value.ToString() != "")
                {
                    // 通过属性类型 对取到的值做对应的转换
                    string typeName = prop.PropertyType.Name;
                    if (typeName == typeof(string).Name)
                    {
                        prop.SetValue(result, value.ToString(), null);
                    }
                    else if (typeName == typeof(int).Name)
                    {
                        prop.SetValue(result, Convert.ToInt32(value), null);
                    }
                    else if (typeName == typeof(DateTime).Name)
                    {
                        prop.SetValue(result, Convert.ToDateTime(value), null);
                    }
                    else if (typeName == typeof(Guid).Name)
                    {
                        prop.SetValue(result, Guid.Parse(value.ToString()), null);
                    }
                    else if (typeName == typeof(Int64).Name)
                    {
                        prop.SetValue(result, Convert.ToInt64(value), null);
                    }
                    else if (typeName == typeof(bool).Name)
                    {
                        prop.SetValue(result, Convert.ToBoolean(value), null);
                    }
                    else if (typeName == typeof(decimal).Name)
                    {
                        prop.SetValue(result, Convert.ToDecimal(value), null);
                    }
                    else
                    {
                        prop.SetValue(result, value, null);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 解析为数组
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="value">值</param>
        /// <param name="split">分割符</param>
        /// <returns>结果</returns>
        public static T[] ParseArray<T>(string value, char split = '|')
        {
            var type = typeof(T);
            T[] array = null;
            if (!string.IsNullOrEmpty(value))
            {
                array = value.Split(split).Select(item =>
                {
                    if (type.IsEnum)
                    {
                        return (T)Enum.Parse(type, item, true);
                    }
                    else
                    {
                        return (T)Convert.ChangeType(item, typeof(T));
                    }
                }).ToArray();
            }
            return array;
        }
        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为Int32类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static int TryToInt32(object input, int defaultValue = 0)
        {
            try
            {
                return Convert.ToInt32(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为Int16类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Int16 TryToInt16(object input, Int16 defaultValue = 0)
        {
            try
            {
                return Convert.ToInt16(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为Int64类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Int64 TryToInt64(object input, Int64 defaultValue = 0)
        {
            try
            {
                return Convert.ToInt64(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 Decimal 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Decimal TryToDecimal(object input, Decimal defaultValue = 0)
        {
            try
            {
                return Convert.ToDecimal(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 Double 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Double TryToDouble(object input, Double defaultValue = 0.00)
        {
            try
            {
                return Convert.ToDouble(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 String 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static String TryToString(object input, String defaultValue = "NULL")
        {
            try
            {
                return Convert.ToString(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 Char 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Char TryToChar(object input, Char defaultValue = 'N')
        {
            try
            {
                return Convert.ToChar(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 DateTime类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static DateTime TryToDateTime(object input, DateTime defaultValue = default(DateTime))
        {
            try
            {
                return Convert.ToDateTime(input);
            }
            catch
            {
                return defaultValue;
                //return new DateTime((long)0);
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 Boolean 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Boolean TryToBool(object input, Boolean defaultValue = false)
        {
            try
            {
                return Convert.ToBoolean(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 Byte 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Byte TryToByte(object input, Byte defaultValue = 0)
        {
            try
            {
                return Convert.ToByte(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 SByte 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static SByte TryToSByte(object input, SByte defaultValue = 0)
        {
            try
            {
                return Convert.ToSByte(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 Single 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static Single TryToSingle(object input, Single defaultValue = 0)
        {
            try
            {
                return Convert.ToSingle(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 UInt32 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static UInt32 TryToUInt32(object input, UInt32 defaultValue = 0)
        {
            try
            {
                return Convert.ToUInt32(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 UInt16 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static UInt16 TryToUInt16(object input, UInt16 defaultValue = 0)
        {
            try
            {
                return Convert.ToUInt16(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 尝试转换（失败则返回默认值）-- 转换为 UInt64 类型
        /// </summary>
        /// <param name="input">输入</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>结果</returns>
        public static UInt64 TryToUInt64(object input, UInt64 defaultValue = 0)
        {
            try
            {
                return Convert.ToUInt64(input);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="input">输入</param>
        /// <returns></returns>
        public static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            char[] original = input.ToCharArray();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < original.Length; i++)
            {
                if (i == 0) { sb.Append(original[i].ToString().ToUpper()); }
                else
                {
                    sb.Append(original[i].ToString());
                }
            }
            return sb.ToString();
        }

        public static string GetFloatWithoutPoint(string floatValueStr)
        {
            if (floatValueStr.Contains('.'))
            {
                return floatValueStr.TrimEnd('0').TrimEnd('.');
            }
            else
            {
                return floatValueStr;
            }
        }

    }
}
