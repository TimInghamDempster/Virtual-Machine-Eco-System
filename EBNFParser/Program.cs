using System;
using System.IO;
using System.Linq;
using System.Text;

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

            var terminalStringBuilder = new StringBuilder("let patterns = [\n");

            foreach(var elementTuple in parser.Terminals)
            {
                terminalStringBuilder.Append("    (\"");
                terminalStringBuilder.Append(elementTuple.rule.Name);
                terminalStringBuilder.Append("\", ");
                terminalStringBuilder.Append("Regex(\"^");
                terminalStringBuilder.Append(elementTuple.element.Name);
                terminalStringBuilder.Append("\"));\n");
            }

            terminalStringBuilder.Append("]");

            using(var writer = new StreamWriter(args[0]))
            {
                writer.WriteLine("module Terminals");
                writer.WriteLine();
                writer.WriteLine("open System.Text.RegularExpressions");
                writer.WriteLine();
                writer.WriteLine(terminalStringBuilder.ToString());
            }
        }
    }
}
