using System;
using EBNFParser;
using FluentAssertions;
using Xunit;
using System.Linq;

namespace EBNFParserTests
{
    public class ProductionRuleTests
    {
        private readonly string _testRule = "a ==> ^b$ | ^c$ | ^d$ | ^e$";
        private readonly CompositionRoot _root = new CompositionRoot();

        [Fact]
        public void ProductionRuleParsesName()
        {
            var productionRule = _root.ProductionRuleFactory(_testRule);

            productionRule.Name.Should().Be("a");
        }

        [Fact]
        public void ProductionRuleProducesCorrectNumberOfPatterns()
        {
            var productionRule = _root.ProductionRuleFactory(_testRule);

            productionRule.Patterns.Count().Should().Be(4);
        }

        [Fact]
        public void ProductionRuleMustHaveAName()
        {
            var text = "==> | b";
            var productionRule = _root.ProductionRuleFactory(text);

            _root.Logger.Messages.Count().Should().BeGreaterOrEqualTo(1);
            _root.Logger.Messages.Should().Contain((message) => message.Contains("name") && message.Contains(text));
        }

        [Fact]
        public void ProductionRulesMustHaveCorrectSeparator()
        {
            var text = "a b";
            var productionRule = _root.ProductionRuleFactory(text);

            _root.Logger.Messages.Count().Should().BeGreaterOrEqualTo(1);
            _root.Logger.Messages.Should().Contain((message) => message.Contains("separator") && message.Contains(text));
        }

        [Fact]
        public void PipeDoesntSplitPatternsInATerminal()
        {
            var productionRule = _root.ProductionRuleFactory("a ==> ^c| b$");
            productionRule.Patterns.Count().Should().Be(1);
        }

        [Fact]
        public void PatternCannotStartWithPipe()
        {
            var text = "a ==> | b";
            var productionRule = _root.ProductionRuleFactory(text);

            _root.Logger.Messages.Count().Should().BeGreaterOrEqualTo(1);
            _root.Logger.Messages.Should().Contain((message) => message.Contains("|") && message.Contains("starts with") && message.Contains(text));
        }

        [Fact]
        public void EachRuleIsLoggedOnParsing()
        {
            var productionRule = _root.ProductionRuleFactory(_testRule);

            _root.Logger.Messages.Should().Contain(msg => msg.Contains(_testRule));
        }
    }
}
