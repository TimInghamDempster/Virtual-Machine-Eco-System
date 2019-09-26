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

        public class PatternStateMachine
        {
            public enum State
            {
                Initialising,
                ProcessingTerminal,
                ProcessingRule,
                ProcessingRepeat
            }

            public State CurrentState { get; set; }
            public StringBuilder CurrentString { get; set; } = new StringBuilder();

            internal void Reset()
            {
                CurrentString = new StringBuilder();
                CurrentState = State.Initialising;
            }
        }

        public List<GrammarElement> _elements = new List<GrammarElement>();
        public IEnumerable<GrammarElement> Elements => _elements;

        public IEnumerable<GrammarElement> Terminals
        {
            get
            {
                foreach (var element in Elements)
                {
                    if(element.IsTerminal)
                    {
                        yield return element;
                    }
                }
            }
        }

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
                    else if(element.IsRepeat)
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
            Func<string, GrammarElement> regexFactory,
            ILogger logger,
            bool isRepeat)
        {
            _terminalFactory = terminalFactory;
            _ruleFactory = ruleFactory;
            _repeatFactory = repeatFactory;
            _logger = logger;
            _isRepeat = isRepeat;

            _patternString = text;

            if(text.StartsWith("Regex"))
            {
                _elements.Add(regexFactory(text.Replace("Regex", "")));
            }

            var stateMachine= new PatternStateMachine();
            foreach(var character in text)
            {
                ProcessChar(stateMachine, character);
            }

            if(stateMachine.CurrentState == PatternStateMachine.State.ProcessingTerminal)
            {
                _logger.Log($"Error: terminal not closed at end of pattern {text}");
            }

            if (stateMachine.CurrentState == PatternStateMachine.State.ProcessingRepeat)
            {
                _logger.Log($"Error: repeat not closed at end of pattern {text}");
            }

            if(stateMachine.CurrentState == PatternStateMachine.State.ProcessingRule)
            {
                _elements.Add(_ruleFactory(stateMachine.CurrentString.ToString()));
            }
        }

        private void ProcessChar(PatternStateMachine stateMachine, char character)
        {
            switch (stateMachine.CurrentState)
            {
                case PatternStateMachine.State.Initialising:
                    ProcessInitialChar(stateMachine, character);
                    break;
                case PatternStateMachine.State.ProcessingTerminal:
                    ProcessTerminalChar(stateMachine, character);
                    break;
                case PatternStateMachine.State.ProcessingRule:
                    ProcessRuleChar(stateMachine, character);
                    break;
                case PatternStateMachine.State.ProcessingRepeat:
                    ProcessRepeatChar(stateMachine, character);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessRepeatChar(PatternStateMachine stateMachine, char character)
        {
            switch(character)
            {
                case '}':
                    _elements.Add(_repeatFactory(stateMachine.CurrentString.ToString()));
                    stateMachine.Reset();
                    break;
                default:
                    stateMachine.CurrentString.Append(character);
                    break;
            }
        }

        // Rules are split by spaces, so keep consuming chars until we find one
        private void ProcessRuleChar(PatternStateMachine stateMachine, char character)
        {
            switch(character)
            {
                case ' ':
                    _elements.Add(_ruleFactory(stateMachine.CurrentString.ToString()));
                    stateMachine.Reset();
                    break;
                case '{':
                    TryEnterRepeatProcessing(stateMachine);
                    break;
                case '}':
                    stateMachine.Reset();
                    _logger.Log($"Error: attempted to close a reapeat that does not exist in pattern {_patternString}");
                    break;
                default:
                    stateMachine.CurrentString.Append(character);
                    break;
            }
        }

        private void TryEnterRepeatProcessing(PatternStateMachine stateMachine)
        {
            if (_isRepeat == false)
            {
                stateMachine.CurrentState = PatternStateMachine.State.ProcessingRepeat;
            }
            else
            {
                stateMachine.Reset();
                _logger.Log($"Error: repeat sequences cannot be nested, atempted repeat {_patternString}");
            }
        }

        // We are now in a terminal, keep going until we meet the closing
        // char or the end of the text
        private void ProcessTerminalChar(PatternStateMachine stateMachine, char character)
        {
            switch (character)
            {
                case '\"':
                    _elements.Add(_terminalFactory(stateMachine.CurrentString.ToString()));
                    stateMachine.Reset();
                    break;
                default:
                    stateMachine.CurrentString.Append(character);
                    break;
            }
        }

        private void ProcessInitialChar(PatternStateMachine stateMachine, char character)
        {
            switch (character)
            {
                case '\"':
                    stateMachine.CurrentState = PatternStateMachine.State.ProcessingTerminal;
                    break;
                case '{':
                    TryEnterRepeatProcessing(stateMachine);
                    break;
                case '}':
                    stateMachine.CurrentState = PatternStateMachine.State.Initialising;
                    _logger.Log("Error: first character in pattern cannot end a repeat");
                    break;
                case ' ':
                    break;
                default:
                    stateMachine.CurrentState = PatternStateMachine.State.ProcessingRule;
                    stateMachine.CurrentString = new StringBuilder(character.ToString());
                    break;
            }
        }
    }
}