using Xunit;
using FluentAssertions;
using System.Linq;

namespace EBNFParserTests
{
    public class GrammarParserTests
    {
        private string testGrammar = "a ==> b\nb==>\"c\"\nd ==> \"e\" | \"f\"";
        private readonly CompositionRoot _root = new CompositionRoot();

        [Fact]
        public void TextIsTurnedIntoProductionRules()
        {
            var parser = _root.ParserFactory(testGrammar);

            parser.ProductionRules.Should().HaveCount(3);
        }

        [Fact]
        public void AllTerminalsCanBeExtractedFromGrammar()
        {
            var parser = _root.ParserFactory(testGrammar);

            parser.Terminals.Should().Contain((element) => element.Name == "c" && element.IsTerminal);
            parser.Terminals.Should().Contain((element) => element.Name == "e" && element.IsTerminal);
            parser.Terminals.Should().Contain((element) => element.Name == "f" && element.IsTerminal);
            parser.Terminals.Should().OnlyContain((element) => element.IsTerminal);
        }

        [Fact]
        public void LeftRecusrionIsReported()
        {
            var leftRecursiveGrammar = "a ==> b\nb==> c \n c ==> d\n d ==> a";

            var parser = _root.ParserFactory(leftRecursiveGrammar);

            _root.Logger.Messages.Should().Contain(msg => msg.Contains("left-recursive") && msg.Contains("a"));
        }

        [Fact]
        public void NonLeftRecursionIsAllowed()
        {
            var recursiveGrammar = "a ==> b\nb==>\"c\"\nc ==> \"e\" a | \"f\"";
            var parser = _root.ParserFactory(recursiveGrammar);

            _root.Logger.Messages.Count().Should().Be(0);
        }

        [Fact]
        public void MissingRulesAreReported()
        {
            var incompleteGrammar = "a ==> b\nc ==> \"e\" a | \"f\"";
            var parser = _root.ParserFactory(incompleteGrammar);

            _root.Logger.Messages.Count().Should().BeGreaterOrEqualTo(1);
            _root.Logger.Messages.Should().Contain(msg => msg.Contains("b") && msg.Contains("not defined"));
        }
    }
}
