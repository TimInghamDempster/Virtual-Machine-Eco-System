namespace Compiler
{
    public interface ILogger
    {
        bool HasLogged { get; }
        void Log(string message);
    }
}
