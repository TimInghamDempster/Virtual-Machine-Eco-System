using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBNFParser
{
    public class ProductionRule
    {
        public class RuleStateMachine
        {
            public enum State
            {
                Initialising,
                ProcessingTerminal,
                ProcessingNonTerminal
            }

            public State CurrentState { get; set; }
            public StringBuilder CurrentString { get; set; } = new StringBuilder();

            internal void Reset()
            {
                CurrentString = new StringBuilder();
                CurrentState = State.Initialising;
            }
        }

        private readonly string _ruleString;

        private readonly Func<string, Pattern> _patternFactory;
        private readonly List<Pattern> _patterns = new List<Pattern>();
        public IEnumerable<Pattern> Patterns => _patterns;

        public ILogger Logger;

        public IEnumerable<GrammarElement> Terminals
        {
            get
            {
                foreach (var pattern in _patterns)
                {
                    foreach (var element in pattern.Terminals)
                    {
                        yield return element;
                    }
                }
            }
        }

        public string Name { get; }

        public ProductionRule(string line, Func<string, Pattern> patternFactory, ILogger logger)
        {
            _ruleString = line;
            _patternFactory = patternFactory;
            Logger = logger;

            const int rulePartsCount = 2; // name, rule
            var parts = line.Split("==>");

            if (parts.Length != rulePartsCount)
            {
                logger.Log($"Production rule {line} does not contain a separator");
                return;
            }

            Name = parts.First().Replace(" ", "");

            if(Name == "")
            {
                logger.Log($"Error: production rule {line} does not have a name");
            }

            var stateMachine = new RuleStateMachine();
            foreach(var character in parts[1])
            {
                ProcessCharacter(stateMachine, character);
            }

            _patterns.Add(_patternFactory(stateMachine.CurrentString.ToString()));
        }

        public override string ToString()
        {
            return _ruleString;
        }

        private void ProcessCharacter(RuleStateMachine stateMachine, char character)
        {
            switch (stateMachine.CurrentState)
            {
                case RuleStateMachine.State.Initialising:
                    ProcessInitialChar(stateMachine, character);
                    break;
                case RuleStateMachine.State.ProcessingTerminal:
                    ProcessTerminalChar(stateMachine, character);
                    break;
                case RuleStateMachine.State.ProcessingNonTerminal:
                    ProcessNonTerminalChar(stateMachine, character);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ProcessInitialChar(RuleStateMachine stateMachine, char character)
        {
            switch(character)
            {
                case '|':
                    Logger.Log($"Error: Pattern in rule {_ruleString} starts with a | which is not legal");
                    break;
                case ' ':
                    break;
                case '\"':
                    stateMachine.CurrentState = RuleStateMachine.State.ProcessingTerminal;
                    stateMachine.CurrentString.Append(character);
                    break;
                default:
                    stateMachine.CurrentState = RuleStateMachine.State.ProcessingNonTerminal;
                    stateMachine.CurrentString.Append(character);
                    break;
            }
        }

        private void ProcessTerminalChar(RuleStateMachine stateMachine, char character)
        {
            switch (character)
            {
                case '\"':
                    stateMachine.CurrentState = RuleStateMachine.State.ProcessingNonTerminal;
                    stateMachine.CurrentString.Append(character);
                    break;
                default:
                    stateMachine.CurrentString.Append(character);
                    break;
            }
        }

        private void ProcessNonTerminalChar(RuleStateMachine stateMachine, char character)
        {
            switch (character)
            {
                case '\"':
                    stateMachine.CurrentState = RuleStateMachine.State.ProcessingTerminal;
                    stateMachine.CurrentString.Append(character);
                    break;
                case '|':
                    stateMachine.CurrentState = RuleStateMachine.State.Initialising;
                    _patterns.Add(_patternFactory(stateMachine.CurrentString.ToString()));
                    stateMachine.CurrentString = new StringBuilder();
                    break;
                default:
                    stateMachine.CurrentString.Append(character);
                    break;
            }
        }
    }
}