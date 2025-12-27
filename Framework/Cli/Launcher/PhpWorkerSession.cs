using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace PhpCompiler
{
    internal sealed class PhpWorkerSession : IPhpUiSession, IDisposable
    {
        private readonly LauncherLog _logger;
        private readonly UiPayloadParser _parser;
        private readonly string _phpExe;
        private readonly string _script;
        private readonly string _errorLogPath;
        private readonly bool _debug;
        private readonly bool _debugTree;
        private readonly JavaScriptSerializer _serializer;
        private Process _process;
        private StreamWriter _stdin;
        private StreamReader _stdout;
        private Thread _stderrThread;

        public PhpWorkerSession(
            LauncherLog logger,
            UiPayloadParser parser,
            string phpExe,
            string script,
            string errorLogPath,
            bool debug,
            bool debugTree)
        {
            _logger = logger;
            _parser = parser;
            _phpExe = phpExe;
            _script = script;
            _errorLogPath = errorLogPath;
            _debug = debug;
            _debugTree = debugTree;
            _serializer = new JavaScriptSerializer();
        }

        public void Start()
        {
            if (_process != null && !_process.HasExited)
            {
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = _phpExe,
                Arguments = PhpArgumentsBuilder.BuildWorker(_script, _errorLogPath),
                WorkingDirectory = Path.GetDirectoryName(_script) ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            _process = Process.Start(psi);
            if (_process == null)
            {
                throw new InvalidOperationException("Failed to start PHP worker process.");
            }

            _stdin = _process.StandardInput;
            _stdout = _process.StandardOutput;

            _stderrThread = new Thread(ReadStdErr) { IsBackground = true };
            _stderrThread.Start();
        }

        public UiPayload Execute(string[] args, out int exitCode)
        {
            Start();

            var message = new WorkerRequest(args ?? Array.Empty<string>());
            string line = _serializer.Serialize(message);
            _stdin.WriteLine(line);
            _stdin.Flush();

            if (_debug && !_debugTree)
            {
                _logger.Log("Worker request args: " + string.Join(" ", message.Args));
            }

            string output = ReadResponseLine();
            if (output == null)
            {
                exitCode = _process.HasExited ? _process.ExitCode : 4;
                return new UiPayload { Text = "PHP worker stopped unexpectedly." };
            }

            exitCode = 0;
            UiPayload payload = _parser.Parse(output);
            if (payload == null)
            {
                payload = new UiPayload
                {
                    Text = string.IsNullOrWhiteSpace(output)
                        ? "No output was produced by the PHP worker."
                        : output.TrimEnd()
                };
            }

            if (_debug)
            {
                _logger.Log(string.Format(
                    "Worker payload: text={0}, nodes={1}",
                    payload.Text == null ? "null" : payload.Text.Length.ToString(),
                    payload.Nodes == null ? "null" : "present"));
            }

            return payload;
        }

        private string ReadResponseLine()
        {
            string line;
            while ((line = _stdout.ReadLine()) != null)
            {
                if (line.Length == 0)
                {
                    continue;
                }
                return line;
            }

            return null;
        }

        private void ReadStdErr()
        {
            try
            {
                string line;
                while (_process != null && !_process.HasExited && (line = _process.StandardError.ReadLine()) != null)
                {
                    if (_debug && line.Length > 0)
                    {
                        _logger.Log("PHP stderr: " + line);
                    }
                }
            }
            catch
            {
                // Ignore stderr reader errors.
            }
        }

        public void Dispose()
        {
            try
            {
                if (_stdin != null && !_stdin.BaseStream.CanWrite)
                {
                    return;
                }

                if (_stdin != null)
                {
                    try
                    {
                        _stdin.WriteLine(_serializer.Serialize(new WorkerRequest("exit")));
                        _stdin.Flush();
                    }
                    catch
                    {
                        // Ignore send failures.
                    }
                }
            }
            finally
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                }
                _process = null;
            }
        }

        private sealed class WorkerRequest
        {
            public WorkerRequest(string[] args)
            {
                Args = args;
            }

            public WorkerRequest(string command)
            {
                Command = command;
                Args = Array.Empty<string>();
            }

            public string Command { get; set; }
            public string[] Args { get; set; }
        }
    }
}
