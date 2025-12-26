using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PhpCompiler
{
    internal static class UiNodeReader
    {
        public static string GetString(Dictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
            {
                return null;
            }
            return dict[key].ToString();
        }

        public static bool GetBool(Dictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
            {
                return false;
            }
            bool result;
            return bool.TryParse(dict[key].ToString(), out result) && result;
        }

        public static object[] GetArray(Dictionary<string, object> dict, string key)
        {
            if (dict == null || !dict.ContainsKey(key) || dict[key] == null)
            {
                return new object[0];
            }

            object value = dict[key];
            var array = value as object[];
            if (array != null)
            {
                return array;
            }

            var list = value as ArrayList;
            if (list != null)
            {
                return list.Cast<object>().ToArray();
            }

            var enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string) && !(value is IDictionary))
            {
                var items = new List<object>();
                foreach (var item in enumerable)
                {
                    items.Add(item);
                }
                return items.ToArray();
            }

            return new object[0];
        }
    }
}
