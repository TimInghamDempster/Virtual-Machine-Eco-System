using System.Collections.Generic;

namespace EBNFParser
{
    public interface ILogger
    {
        void Log(string message);

        IEnumerable<string> Messages {get;}
    }
}