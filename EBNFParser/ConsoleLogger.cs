using System;
using System.Collections.Generic;
using System.Text;

namespace EBNFParser
{
    public class ConsoleLogger : ILogger
    {
        private readonly List<string> _messages = new List<string>();
        public void Log(string message)
        {
            Console.WriteLine(message);
            _messages.Add(message);
        }

        public IEnumerable<string> Messages => _messages;

    }
}
