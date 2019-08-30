using System;

namespace Compiler
{
    public class ConsoleLogger : ILogger
    {
        public bool HasLogged { get; private set; } = false;

        public void Log(string message)
        {
            HasLogged = true;
            Console.WriteLine(message);
        }
    }
}
