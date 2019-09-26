using System;

namespace EBNFParser
{
    public class CompositionRoot
    {
        public ILogger Logger {get; protected set;} = new ConsoleLogger();

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
                RegexElementFactory,
                Logger,
                false);

        public Func<string, Pattern> RepeatPatternFactory =>
            (patternString) => 
            new Pattern(
                patternString,
                TerminalFactory, 
                RuleElementFactory,
                RepeatElementFactory, 
                RegexElementFactory,
                Logger, true);

        public Func<string, GrammarElement> TerminalFactory =>
            (terminalString) => new GrammarElement(terminalString, ElementType.Terminal);

        public Func<string, GrammarElement> RuleElementFactory =>
            (ruleString) => new GrammarElement(ruleString, ElementType.Rule);

        public Func<string, GrammarElement> RepeatElementFactory =>
            (repeatString) => new GrammarElement(repeatString, ElementType.Repeat, RepeatPatternFactory(repeatString));

        public Func<string, GrammarElement> RegexElementFactory =>
            (regexString) => new GrammarElement(regexString, ElementType.Regex);
    }
}