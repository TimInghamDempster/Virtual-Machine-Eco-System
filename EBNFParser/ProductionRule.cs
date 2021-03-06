using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBNFParser
{
    public class ProductionRule
    {
        public enum State
        {
            Initialising,
            ProcessingTerminal,
            ProcessingNonTerminal
        }

        private State _currentState;
        private StringBuilder _currentString  = new StringBuilder();

        private readonly string _ruleString;

        private readonly Func<string, Pattern> _patternFactory;
        private readonly List<Pattern> _patterns = new List<Pattern>();
        public IList<Pattern> Patterns => _patterns;

        public ILogger Logger;

        public IEnumerable<(ProductionRule, GrammarElement)> Terminals =>
            _patterns.SelectMany(p => p.Terminals).
            Select(elm => (this, elm));

        public string Name { get; } = "";

        public ProductionRule(string line, Func<string, Pattern> patternFactory, ILogger logger)
        {
            _ruleString = line;
            _patternFactory = patternFactory;
            Logger = logger;

            logger.Log($"Parsing rule {_ruleString}", LogLevel.Info);

            const int rulePartsCount = 2; // name, rule
            var parts = line.Split("==>");

            if (parts.Length != rulePartsCount)
            {
                logger.Log($"Production rule {line} does not contain a separator", LogLevel.Error);
                return;
            }

            Name = parts.First().Replace(" ", "");

            if(Name == "")
            {
                logger.Log($"Error: production rule {line} does not have a name", LogLevel.Error);
            }

            foreach(var character in parts[1])
            {
                ProcessCharacter(character);
            }

            _patterns.Add(_patternFactory(_currentString.ToString()));

            if(_patterns.Count > 1 && _patterns.Any(p => p.Terminals.Any()))
            {
                logger.Log($"Error: production rule {line} defines a terminal and so can only have one pattern", LogLevel.Error);
            }
        }

        public override string ToString()
        {
            return _ruleString;
        }

        private void ProcessCharacter(char character)
        {
            switch (_currentState)
            {
                case State.Initialising:
                    ProcessInitialChar(character);
                    break;
                case State.ProcessingTerminal:
                    ProcessTerminalChar(character);
                    break;
                case State.ProcessingNonTerminal:
                    ProcessNonTerminalChar(character);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ProcessInitialChar(char character)
        {
            switch(character)
            {
                case '|':
                    Logger.Log($"Error: Pattern in rule {_ruleString} starts with a | which is not legal", LogLevel.Error);
                    break;
                case ' ':
                    break;
                case '^':
                    _currentState = State.ProcessingTerminal;
                    _currentString.Append(character);
                    break;
                default:
                    _currentState = State.ProcessingNonTerminal;
                    _currentString.Append(character);
                    break;
            }
        }

        private void ProcessTerminalChar(char character)
        {
            switch (character)
            {
                case '$':
                    _currentState = State.ProcessingNonTerminal;
                    _currentString.Append(character);
                    break;
                default:
                    _currentString.Append(character);
                    break;
            }
        }

        private void ProcessNonTerminalChar(char character)
        {
            switch (character)
            {
                case '^':
                    _currentState = State.ProcessingTerminal;
                    _currentString.Append(character);
                    break;
                case '|':
                    _currentState = State.Initialising;
                    _patterns.Add(_patternFactory(_currentString.ToString()));
                    _currentString = new StringBuilder();
                    break;
                default:
                    _currentString.Append(character);
                    break;
            }
        }
    }
}