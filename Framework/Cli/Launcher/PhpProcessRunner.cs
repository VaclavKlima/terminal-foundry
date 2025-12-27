using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PhpCompiler
{
    internal sealed class PhpProcessRunner
    {
        public PhpExecutionResult Execute(PhpExecutionRequest request)
        {
            var psi = new ProcessStartInfo
            {
                FileName = request.PhpExe,
                Arguments = PhpArgumentsBuilder.Build(request.Script, request.ErrorLogPath, request.Args),
                WorkingDirectory = Path.GetDirectoryName(request.Script) ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var stopwatch = Stopwatch.StartNew();
            using (var process = Process.Start(psi))
            {
                if (process == null)
                {
                    return new PhpExecutionResult(4, string.Empty, "Failed to start PHP process.", 0);
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                stopwatch.Stop();

                return new PhpExecutionResult(process.ExitCode, output, error, stopwatch.ElapsedMilliseconds);
            }
        }
    }

    internal sealed class PhpExecutionRequest
    {
        public PhpExecutionRequest(string phpExe, string script, string errorLogPath, string[] args)
        {
            PhpExe = phpExe;
            Script = script;
            ErrorLogPath = errorLogPath;
            Args = args ?? Array.Empty<string>();
        }

        public string PhpExe { get; private set; }
        public string Script { get; private set; }
        public string ErrorLogPath { get; private set; }
        public string[] Args { get; private set; }
    }

    internal sealed class PhpExecutionResult
    {
        public PhpExecutionResult(int exitCode, string output, string error, long durationMs)
        {
            ExitCode = exitCode;
            Output = output ?? string.Empty;
            Error = error ?? string.Empty;
            DurationMs = durationMs;
        }

        public int ExitCode { get; private set; }
        public string Output { get; private set; }
        public string Error { get; private set; }
        public long DurationMs { get; private set; }
    }
}
