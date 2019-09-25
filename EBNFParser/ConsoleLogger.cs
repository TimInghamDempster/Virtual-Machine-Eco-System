using System;
using System.Collections.Generic;
using System.Text;

namespace EBNFParser
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public IEnumerable<string> Messages => null;
    }
}
