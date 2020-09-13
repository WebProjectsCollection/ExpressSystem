using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace ExpressSystem.Api.Utilities
{
    public class XmlEntityExchange<T> where T : new()
    {
        /// <summary>
        /// 将XML转换为对象
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T ConvertXmlToEntity(string xml)
        {
            XmlDocument doc = new XmlDocument();
            PropertyInfo[] propinfos = null;
            doc.LoadXml(xml);
            XmlNodeList nodelist = doc.SelectNodes("/xml");
            T entity = new T();
            foreach (XmlNode node in nodelist)
            {
                //初始化propertyinfo
                if (propinfos == null)
                {
                    Type objtype = entity.GetType();
                    propinfos = objtype.GetProperties();
                }
                //填充entity类的属性
                foreach (PropertyInfo pi in propinfos)
                {
                    XmlNode cnode = node.SelectSingleNode(pi.Name);
                    if (cnode != null)
                    {
                        pi.SetValue(entity, Convert.ChangeType(cnode.InnerText, pi.PropertyType), null);
                    }
                }
            }
            return entity;
        }
    }
}
