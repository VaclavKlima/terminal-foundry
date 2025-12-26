using System;
using System.IO;

namespace PhpCompiler
{
    internal sealed class LauncherLog
    {
        private readonly string _logPath;

        public LauncherLog(string logPath)
        {
            _logPath = logPath;
        }

        public void Log(string message)
        {
            string line = string.Format(
                "[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}",
                DateTime.Now,
                message,
                Environment.NewLine);
            File.AppendAllText(_logPath, line);
        }
    }
}
