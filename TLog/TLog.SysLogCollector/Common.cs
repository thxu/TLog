using Newtonsoft.Json;
using System;
using System.IO;

namespace TLog.SysLogCollector
{
    public static class Common
    {
        /// <summary>
        /// object序列化JSON字符串扩展方法
        /// </summary>
        /// <param name="obj">object及其子类对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            try
            {
                JsonSerializerSettings settting = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd HH:mm:ss" };
                return JsonConvert.SerializeObject(obj, settting).FormatJsonString();
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 格式化json字符串
        /// </summary>
        /// <param name="str">json字符串</param>
        /// <returns>格式化后的json字符串</returns>
        public static string FormatJsonString(this string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }
}
