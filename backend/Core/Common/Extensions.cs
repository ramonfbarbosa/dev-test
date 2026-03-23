using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;

namespace Common
{
    public static class Extensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attributes = field?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes?.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : value.ToString() ?? value.ToString();
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
        }

        public static string ToJson(this object obj, bool igonoreNull = false)
        {
            if (igonoreNull)
                return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, DateTimeZoneHandling = DateTimeZoneHandling.Utc });

            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, DateTimeZoneHandling = DateTimeZoneHandling.Utc });
        }

        public static string ToIndentedJson(this object obj, bool igonoreNull = false)
        {
            if (igonoreNull)
                return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, DateTimeZoneHandling = DateTimeZoneHandling.Utc });

            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore, DateTimeZoneHandling = DateTimeZoneHandling.Utc });
        }

        public static T FromJson<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
        }

        public static bool TryParseJson<T>(this string value, out T result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public static Guid ToGuid(this string value)
        {
            return new Guid(value);
        }

        public static string UnMask(this string value)
        {
            return string.IsNullOrEmpty(value) ? value : new string(value.Where(char.IsDigit).ToArray());
        }

    }
}
