using System;
using System.Diagnostics;
using System.IO;

namespace PhpCompiler
{
    internal sealed class LauncherApp
    {
        public int Run(string[] args)
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string exeDir = Path.GetDirectoryName(exePath) ?? Environment.CurrentDirectory;
            string logPath = Path.Combine(exeDir, "launcher.log");
            string errorLogPath = Path.Combine(exeDir, "php-error.log");

            var logger = new LauncherLog(logPath);
            var env = EnvLoader.Load(Path.Combine(exeDir, ".env"));
            bool debugUi = EnvLoader.ReadBool(env, "LAUNCHER_DEBUG", false);

            try
            {
                logger.Log("Launcher start");

                string phpExe = PhpLocator.FindPhp(exeDir);
                string script = Path.Combine(exeDir, "index.php");

                logger.Log(string.Format("Using php binary: {0}", phpExe ?? "(missing)"));
                logger.Log(string.Format("Script path: {0}", script));
                logger.Log(string.Format("Args: {0}", PhpArgumentsBuilder.JoinArgs(args)));
                logger.Log(string.Format("PHP error log: {0}", errorLogPath));

                if (phpExe == null || !File.Exists(phpExe))
                {
                    logger.Log("Missing php-win.exe/php.exe next to the launcher.");
                    return 2;
                }

                if (!File.Exists(script))
                {
                    logger.Log("Missing index.php next to the launcher.");
                    return 3;
                }

                var session = new PhpUiSession(
                    logger,
                    new PhpProcessRunner(),
                    new UiPayloadParser(),
                    phpExe,
                    script,
                    errorLogPath,
                    debugUi);

                int exitCode;
                UiPayload payload = session.Execute(args, out exitCode);

                var window = new UiWindow(session, args, logger, debugUi);
                window.Show(payload, "PhpCompiler");

                return exitCode;
            }
            catch (Exception ex)
            {
                logger.Log("Unhandled exception: " + ex);
                return 5;
            }
        }
    }
}
