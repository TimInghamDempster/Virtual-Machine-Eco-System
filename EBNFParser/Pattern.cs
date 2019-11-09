using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EBNFParser
{
    public class Pattern
    {
        private readonly Func<string, GrammarElement> _terminalFactory;
        private readonly Func<global::System.String, GrammarElement> _ruleFactory;
        private readonly Func<string, GrammarElement> _repeatFactory;
        private readonly ILogger _logger;
        private readonly bool _isRepeat;
        private readonly string _patternString;

        public enum State
        {
            Initialising,
            ProcessingTerminal,
            ProcessingRule,
            ProcessingRepeat
        }

        private State _currentState;
        private StringBuilder _currentString = new StringBuilder();

        private void Reset()
        {
            _currentString = new StringBuilder();
            _currentState = State.Initialising;
        }

        public List<GrammarElement> _elements = new List<GrammarElement>();
        public IEnumerable<GrammarElement> Elements => _elements;

        public IEnumerable<GrammarElement> Terminals
            => Elements.Where(element => element.IsTerminal);

        public IEnumerable<string> Rules
        {
            get
            {
                foreach(var element in Elements)
                {
                    if(element.IsRule)
                    {
                        yield return element.Name;
                    }
                    else if(element.IsRepeat && element.SubPattern != null)
                    {
                        foreach(var rule in element.SubPattern.Rules)
                        {
                            yield return rule;
                        }
                    }
                }
            }
        }

        public Pattern(
            string text, 
            Func<string, GrammarElement> terminalFactory,
            Func<string, GrammarElement> ruleFactory,
            Func<string, GrammarElement> repeatFactory,
            ILogger logger,
            bool isRepeat)
        {
            _terminalFactory = terminalFactory;
            _ruleFactory = ruleFactory;
            _repeatFactory = repeatFactory;
            _logger = logger;
            _isRepeat = isRepeat;

            _patternString = text;

            foreach(var character in text)
            {
                ProcessChar(character);
            }

            if(_currentState == State.ProcessingTerminal)
            {
                _logger.Log($"Error: terminal not closed at end of pattern {text}");
            }

            if (_currentState == State.ProcessingRepeat)
            {
                _logger.Log($"Error: repeat not closed at end of pattern {text}");
            }

            if(_currentState == State.ProcessingRule)
            {
                _elements.Add(_ruleFactory(_currentString.ToString()));
            }
        }

        private void ProcessChar(char character)
        {
            switch (_currentState)
            {
                case State.Initialising:
                    ProcessInitialChar(character);
                    break;
                case State.ProcessingTerminal:
                    ProcessTerminalChar(character);
                    break;
                case State.ProcessingRule:
                    ProcessRuleChar(character);
                    break;
                case State.ProcessingRepeat:
                    ProcessRepeatChar(character);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessRepeatChar(char character)
        {
            switch(character)
            {
                case '}':
                    _elements.Add(_repeatFactory(_currentString.ToString()));
                    Reset();
                    break;
                default:
                    _currentString.Append(character);
                    break;
            }
        }

        // Rules are split by spaces, so keep consuming chars until we find one
        private void ProcessRuleChar(char character)
        {
            switch(character)
            {
                case ' ':
                    _elements.Add(_ruleFactory(_currentString.ToString()));
                    Reset();
                    break;
                case '{':
                    TryEnterRepeatProcessing();
                    break;
                case '}':
                    Reset();
                    _logger.Log($"Error: attempted to close a reapeat that does not exist in pattern {_patternString}");
                    break;
                default:
                    _currentString.Append(character);
                    break;
            }
        }

        private void TryEnterRepeatProcessing()
        {
            if (_isRepeat == false)
            {
                _currentState = State.ProcessingRepeat;
            }
            else
            {
                Reset();
                _logger.Log($"Error: repeat sequences cannot be nested, atempted repeat {_patternString}");
            }
        }

        // We are now in a terminal, keep going until we meet the closing
        // char or the end of the text
        private void ProcessTerminalChar(char character)
        {
            switch (character)
            {
                case '$':
                    _elements.Add(_terminalFactory(_currentString.ToString()));
                    Reset();
                    break;
                default:
                    _currentString.Append(character);
                    break;
            }
        }

        private void ProcessInitialChar(char character)
        {
            switch (character)
            {
                case '^':
                    _currentState = State.ProcessingTerminal;
                    break;
                case '{':
                    TryEnterRepeatProcessing();
                    break;
                case '}':
                    _currentState = State.Initialising;
                    _logger.Log("Error: first character in pattern cannot end a repeat");
                    break;
                case ' ':
                    break;
                default:
                    _currentState = State.ProcessingRule;
                    _currentString = new StringBuilder(character.ToString());
                    break;
            }
        }
    }
}