using System;

namespace PhpCompiler
{
    internal static class Launcher
    {
        [STAThread]
        private static int Main(string[] args)
        {
            var app = new LauncherApp();
            return app.Run(args);
        }
    }
}
