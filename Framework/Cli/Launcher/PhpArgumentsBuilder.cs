using System;
using System.Linq;
using System.Text;

namespace PhpCompiler
{
    internal static class PhpArgumentsBuilder
    {
        public static string Build(string script, string errorLogPath, string[] args)
        {
            string[] filtered = args.Where(arg => arg != "--ui-json").ToArray();
            string passthrough = filtered.Length > 0 ? " " + JoinArgs(filtered) : string.Empty;
            return string.Format(
                "-d display_errors=0 -d log_errors=1 -d error_log={0} {1} --ui-json{2}",
                Quote(errorLogPath),
                Quote(script),
                passthrough);
        }

        public static string JoinArgs(string[] args)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(Quote(args[i]));
            }
            return sb.ToString();
        }

        private static string Quote(string value)
        {
            if (value.Length == 0) return "\"\"";
            if (value.IndexOfAny(new[] { ' ', '\t', '\n', '\r', '"' }) == -1) return value;
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }
}
