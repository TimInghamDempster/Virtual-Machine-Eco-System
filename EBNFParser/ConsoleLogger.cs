using System;
using System.Collections.Generic;
using System.Text;

namespace EBNFParser
{
    public class ConsoleLogger : ILogger
    {
        private readonly List<string> _messages = new List<string>();
        private readonly bool _isShushed;

        public ConsoleLogger(bool isShushed)
        {
            _isShushed = isShushed;
        }

        public void Log(string message, LogLevel level)
        {
            if (!_isShushed || level == LogLevel.Error)
            {
                Console.WriteLine(message);
            }
            _messages.Add(message);
        }

        public IEnumerable<string> Messages => _messages;

    }
}
