using System;
using System.Linq;
using EBNFParser;
using FluentAssertions;
using Xunit;

namespace EBNFParserTests
{
    public class PatternTests
    {
        private string _testText = "a ^b$ ^c$ { d e ^f$}";
        private readonly CompositionRoot _root = new CompositionRoot();

        [Fact]
        public void TerminalsAreRecognised()
        {
            var pattern = _root.NormalPatternFactory(_testText);

            pattern.Elements.Should().Contain((elm) => elm.Name == "b" && elm.Type == ElementType.Terminal);

            pattern.Elements.Should().Contain((elm) => elm.Name == "c" && elm.Type == ElementType.Terminal);
        }

        [Fact]
        public void UnclosedTerminalIsReported()
        {
            var newText = _testText + " ^d";

            var pattern =_root.NormalPatternFactory(newText);

            _root.Logger.Messages.Count().Should().BeGreaterOrEqualTo(1);
            _root.Logger.Messages.Should().Contain((message) => message.Contains("terminal") && message.Contains("closed") && message.Contains(_testText));
        }

        [Fact]
        public void RulesAreRecognised()
        {
            var pattern = _root.NormalPatternFactory(_testText);

            pattern.Elements.Should().Contain((elm) => elm.Name == "a" && elm.Type == ElementType.Rule);
        }

        [Fact]
        public void RepeatsAreRecognised()
        {
            var pattern = _root.NormalPatternFactory(_testText);

            pattern.Elements.Should().Contain((elm) => elm.Type == ElementType.Repeat);

            var subPattern = pattern.
                                Elements.
                                First(elm => elm.Type == ElementType.Repeat).
                                SubPattern;

            subPattern.Should().NotBeNull();

            subPattern.Elements.Should().Contain((elm) => elm.Name == "d" && elm.Type == ElementType.Rule);

            subPattern.Elements.Should().Contain((elm) => elm.Name == "e" && elm.Type == ElementType.Rule);

            subPattern.Elements.Should().Contain((elm) => elm.Name == "f" && elm.Type == ElementType.Terminal);
        }

        [Fact]
        public void UnclosedRepeatsAreReported()
        {
            var text = "a ^b$ ^c$ { d e ^f$";

            var pattern = _root.NormalPatternFactory(text);

            _root.Logger.Messages.Count().Should().BeGreaterOrEqualTo(1);
            _root.Logger.Messages.Should().Contain((message) => message.Contains("repeat") && message.Contains("closed") && message.Contains(text));
        }

        [Fact]
        public void NestedRepeatsAreReported()
        {
            var text = "a ^b$ ^c$ { d {e ^f$} }";

            var pattern = _root.NormalPatternFactory(text);

            _root.Logger.Messages.Count().Should().BeGreaterOrEqualTo(1);
            _root.Logger.Messages.Should().Contain((message) => message.Contains("repeat") && message.Contains("nested") && message.Contains("{e ^f$"));
        }

        [Fact]
        public void MultiCharElementsAreRecognised()
        {
            var text = "abc ^1234$";

            var pattern = _root.NormalPatternFactory(text);

            pattern.Elements.Should().Contain(elm => elm.Name == "abc" && elm.Type == ElementType.Rule);
            pattern.Elements.Should().Contain(elm => elm.Name == "1234" && elm.Type == ElementType.Terminal);
        }

        [Fact]
        public void OpenBraceIsAnAcceptableTerminal()
        {
            var text = "^{$";

            var pattern = _root.NormalPatternFactory(text);

            pattern.Terminals.Should().Contain(t => t.Name == "{");
            _root.Logger.Messages.Should().BeEmpty();
        }

        [Fact]
        public void CloseBraceIsAnAcceptableTerminal()
        {
            var text = "^}$";

            var pattern = _root.NormalPatternFactory(text);

            pattern.Terminals.Should().Contain(t => t.Name == "}");
            _root.Logger.Messages.Should().BeEmpty();
        }

        [Fact]
        public void ClosingRuleIsRecognised()
        {
            var text = "a b";

            var pattern = _root.NormalPatternFactory(text);
            pattern.Rules.Count().Should().Be(2);
        }
    }
}