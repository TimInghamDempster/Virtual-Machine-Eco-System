using System;
using System.IO;
using System.Linq;

namespace EBNFParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string text;

            using (var reader = new StreamReader("EBNF.txt"))
            {
                text = reader.ReadToEnd();
            }

            var root = new CompositionRoot();

            var parser = root.ParserFactory(text);

            if(root.Logger.Messages.Any())
            {
                Console.ReadLine();
            }
        }
    }
}
