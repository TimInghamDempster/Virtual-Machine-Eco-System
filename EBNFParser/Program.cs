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
            parser.GenerateLexerCode(args[0]);
        }
    }
}
