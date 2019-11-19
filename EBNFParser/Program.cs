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

            bool shushed = args.Contains("/s");

            var root = new CompositionRoot(shushed);

            var parser = root.ParserFactory(text);

            var compilerGenerator = root.GeneratorFactory(parser);

            compilerGenerator.GenerateLexerCode(args[0]);
            compilerGenerator.GenerateParser(args[1]);
        }
    }
}
