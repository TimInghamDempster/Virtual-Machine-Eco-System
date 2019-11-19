using System.Collections.Generic;

namespace EBNFParser
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public interface ILogger
    {
        void Log(string message, LogLevel level);

        IEnumerable<string> Messages {get;}
    }
}