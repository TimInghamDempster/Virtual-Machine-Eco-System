using System.Collections.Generic;
using EBNFParser;

namespace EBNFParserTests
{
    public class TestLogger : ILogger
    {
        private readonly List<string> _messages = new List<string>();
        public  IEnumerable<string> Messages => _messages;
        public void Log(string message)
        {
            _messages.Add(message);
        }
    }
}
