using System.IO;

namespace PhpCompiler
{
    internal static class PhpLocator
    {
        public static string FindPhp(string exeDir)
        {
            string phpExe = Path.Combine(exeDir, "php.exe");
            if (File.Exists(phpExe))
            {
                return phpExe;
            }

            phpExe = Path.Combine(exeDir, "php-win.exe");
            if (File.Exists(phpExe))
            {
                return phpExe;
            }

            return null;
        }
    }
}
