using System;
using System.Collections.Generic;
using System.IO;

namespace PhpCompiler
{
    internal static class EnvLoader
    {
        public static Dictionary<string, string> Load(string path)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
            {
                return values;
            }

            foreach (string line in File.ReadAllLines(path))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                int index = trimmed.IndexOf('=');
                if (index <= 0)
                {
                    continue;
                }

                string key = trimmed.Substring(0, index).Trim();
                string value = trimmed.Substring(index + 1).Trim();
                values[key] = value;
            }

            return values;
        }

        public static bool ReadBool(Dictionary<string, string> values, string key, bool defaultValue)
        {
            string value;
            if (values != null && values.TryGetValue(key, out value))
            {
                if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "on", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "no", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "off", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return defaultValue;
        }
    }
}
