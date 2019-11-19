using System;

namespace EBNFParser
{
    public class CompositionRoot
    {
        public CompositionRoot(bool shushed)
        {
            Logger = new ConsoleLogger(shushed);
        }

        public ILogger Logger {get; protected set;}

        public Func<string, GrammarParser> ParserFactory =>
            (textToParse) => new GrammarParser(textToParse, ProductionRuleFactory, Logger);

        public Func<string, ProductionRule> ProductionRuleFactory =>
            (productionRuleString) => new ProductionRule(productionRuleString, NormalPatternFactory, Logger);

        public Func<string, Pattern> NormalPatternFactory =>
            (patternString) => 
            new Pattern(
                patternString,
                TerminalFactory, 
                RuleElementFactory,
                RepeatElementFactory,
                Logger,
                false);

        public Func<string, Pattern> RepeatPatternFactory =>
            (patternString) => 
            new Pattern(
                patternString,
                TerminalFactory, 
                RuleElementFactory,
                RepeatElementFactory,
                Logger, 
                true);

        public Func<string, GrammarElement> TerminalFactory =>
            (terminalString) => new GrammarElement(terminalString, ElementType.Terminal);

        public Func<string, GrammarElement> RuleElementFactory =>
            (ruleString) => new GrammarElement(ruleString, ElementType.Rule);

        public Func<string, GrammarElement> RepeatElementFactory =>
            (repeatString) => new GrammarElement(repeatString, ElementType.Repeat, RepeatPatternFactory(repeatString));
    }
}