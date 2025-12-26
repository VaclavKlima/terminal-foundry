using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace PhpCompiler
{
    internal sealed class UiPayloadParser
    {
        public UiPayload Parse(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            output = output.TrimStart('\uFEFF').Trim();
            if (output.Length == 0)
            {
                return null;
            }

            UiPayload payload = TryParseJsonPayload(output);
            if (payload != null)
            {
                return payload;
            }

            int start = output.IndexOf('{');
            int end = output.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                string candidate = output.Substring(start, end - start + 1);
                payload = TryParseJsonPayload(candidate);
                if (payload != null)
                {
                    return payload;
                }
            }

            return null;
        }

        private static UiPayload TryParseJsonPayload(string json)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                var dict = serializer.Deserialize<Dictionary<string, object>>(json);
                if (dict != null)
                {
                    var payload = new UiPayload
                    {
                        Text = GetDictString(dict, "text", "Text"),
                        CurrentApp = GetDictString(dict, "currentApp", "CurrentApp"),
                        Nodes = GetDictDictionary(dict, "nodes", "Nodes")
                    };

                    if (payload.Text != null || payload.Nodes != null || payload.CurrentApp != null)
                    {
                        return payload;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Fall back to direct deserialization.
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<UiPayload>(json);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static string GetDictString(Dictionary<string, object> dict, string key, string fallbackKey)
        {
            object value;
            if (dict.TryGetValue(key, out value) || dict.TryGetValue(fallbackKey, out value))
            {
                return value != null ? value.ToString() : null;
            }

            return null;
        }

        private static Dictionary<string, object> GetDictDictionary(Dictionary<string, object> dict, string key, string fallbackKey)
        {
            object value;
            if (dict.TryGetValue(key, out value) || dict.TryGetValue(fallbackKey, out value))
            {
                var typed = value as Dictionary<string, object>;
                if (typed != null)
                {
                    return typed;
                }
            }

            return null;
        }
    }

    internal sealed class UiPayload
    {
        public string Text { get; set; }
        public string CurrentApp { get; set; }
        public Dictionary<string, object> Nodes { get; set; }
    }
}
