namespace PhpCompiler
{
    internal interface IPhpUiSession
    {
        UiPayload Execute(string[] args, out int exitCode);
    }
}
