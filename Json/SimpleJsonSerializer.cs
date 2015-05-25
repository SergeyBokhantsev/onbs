using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Json
{
    public static class SimpleJsonSerializer
    {
        public static string Serialize(object obj)
        {
            if (obj.GetType().IsPrimitive)
                return obj.ToString();
            else if (obj is string)
            {
                return string.Concat("\"", obj, "\"");
            }
            else
            {
                var props = obj.GetType().GetProperties();

                var body = string.Join(",", props.Select(p => SerializeProperty(p, obj)));

                return string.Concat("{", body, "}");
            }
        }

        public static byte[] Serialize(object obj, Encoding enc)
        {
            var str = Serialize(obj);
            return (enc ?? Encoding.Default).GetBytes(str);
        }

        private static string SerializeProperty(PropertyInfo prop, object obj)
        {
            var value = prop.GetValue(obj, null);

            if (prop.PropertyType.Equals(typeof(int)))
                return string.Format("\"{0}\":{1}", prop.Name, (int)value);
            if (prop.PropertyType.Equals(typeof(double)))
                return string.Format("\"{0}\":{1}", prop.Name, (double)value);
            if (prop.PropertyType.Equals(typeof(bool)))
                return string.Format("\"{0}\":{1}", prop.Name, (bool)value);
            if (prop.PropertyType.Equals(typeof(string)))
                return string.Format("\"{0}\":\"{1}\"", prop.Name, (string)value);
            if (prop.PropertyType.Equals(typeof(object[])))
                return string.Concat("\"", prop.Name, "\":[", string.Join(",", ((object[])value).Select(Serialize)), "]");

            return string.Concat("\"", prop.Name, "\":", Serialize(value));
        }
    }
}
