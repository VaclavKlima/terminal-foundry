using System;

namespace PhpCompiler
{
    internal sealed class PhpUiSession
    {
        private readonly LauncherLog _logger;
        private readonly PhpProcessRunner _runner;
        private readonly UiPayloadParser _parser;
        private readonly string _phpExe;
        private readonly string _script;
        private readonly string _errorLogPath;
        private readonly bool _debug;

        public PhpUiSession(
            LauncherLog logger,
            PhpProcessRunner runner,
            UiPayloadParser parser,
            string phpExe,
            string script,
            string errorLogPath,
            bool debug)
        {
            _logger = logger;
            _runner = runner;
            _parser = parser;
            _phpExe = phpExe;
            _script = script;
            _errorLogPath = errorLogPath;
            _debug = debug;
        }

        public UiPayload Execute(string[] args, out int exitCode)
        {
            var request = new PhpExecutionRequest(_phpExe, _script, _errorLogPath, args);
            PhpExecutionResult result = _runner.Execute(request);
            exitCode = result.ExitCode;

            _logger.Log(string.Format("Process exit code: {0}", result.ExitCode));
            if (_debug)
            {
                _logger.Log(string.Format("PHP stdout length: {0}", result.Output.Length));
                _logger.Log(string.Format("PHP stderr length: {0}", result.Error.Length));
            }
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                _logger.Log("PHP stderr:");
                _logger.Log(result.Error.TrimEnd());
            }

            UiPayload payload = _parser.Parse(result.Output);
            if (payload == null || (payload.Nodes == null && string.IsNullOrWhiteSpace(payload.Text)))
            {
                payload = new UiPayload
                {
                    Text = string.IsNullOrWhiteSpace(result.Output)
                        ? "No output was produced by the PHP script."
                        : result.Output.TrimEnd()
                };
            }
            else if (_debug)
            {
                _logger.Log(string.Format(
                    "Parsed payload: text={0}, nodes={1}",
                    payload.Text == null ? "null" : payload.Text.Length.ToString(),
                    payload.Nodes == null ? "null" : "present"));
            }

            return payload;
        }
    }
}
