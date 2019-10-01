﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EBNFParser
{
    public class GrammarParser
    {
        public GrammarParser(string grammar, Func<string, ProductionRule> ruleFactory, ILogger logger)
        {
            var textRules = grammar.Replace("\r", "").Split('\n');

            foreach (var rule in textRules)
            {
                _productionRules.Add(ruleFactory(rule));
            }

            VerifyAllRulesPresent(logger);

            VerifyGrammarIsNotLeftRecursive(logger);
        }

        private void VerifyGrammarIsNotLeftRecursive(ILogger logger)
        {
            foreach(var rule in _productionRules)
            {
                var finalRule = FindPath(rule, rule);
                if (finalRule != null)
                {
                    logger.Log($"Error: rule {rule.Name} is left-recursive via rule {finalRule}");
                }
            }
        }

        private ProductionRule FindPath(ProductionRule rule1, ProductionRule rule2)
        {
            HashSet<string> exploredRules = new HashSet<string>();

            var result = ExploreRule(rule1, rule2, exploredRules);

            if(result != null)
            {
                return result;
            }

            return null;
        }

        private ProductionRule ExploreRule(ProductionRule currentRule, ProductionRule targetRule, HashSet<string> exploredRules)
        {
            foreach(var pattern in currentRule.Patterns)
            {
                var firstElm = pattern.Elements.FirstOrDefault();
                if (firstElm != null && firstElm.IsRule)
                {
                    if (firstElm.Name == targetRule.Name)
                    {
                        return currentRule;
                    }
                    else if(!exploredRules.Contains(firstElm.Name))
                    {
                        exploredRules.Add(firstElm.Name);

                        var ruleToExplore =
                            ProductionRules.
                            Where(rule => rule.Name == firstElm.Name).
                            FirstOrDefault();

                        if (ruleToExplore != null)
                        {
                            var exploreResult = ExploreRule(ruleToExplore, targetRule, exploredRules);

                            if(exploreResult != null)
                            {
                                return exploreResult;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void VerifyAllRulesPresent(ILogger logger)
        {
            var definedRules = new HashSet<string>();
            foreach(var rule in _productionRules)
            {
                definedRules.Add(rule.Name);
            }

            var requiredRules = new List<Tuple<string,string>>();
            foreach(var rule in _productionRules)
            {
                foreach (var pattern in rule.Patterns)
                {
                    requiredRules.AddRange(
                        pattern.Rules.
                        Select(patternRule =>
                            new Tuple<string, string>(rule.ToString(), patternRule)));
                }
            }

            foreach(var requiredRule in requiredRules)
            {
                if(!definedRules.Contains(requiredRule.Item2))
                {
                    logger.Log($"Error rule {requiredRule.Item2} is referenced in a pattern in rule {requiredRule.Item1} but not defined");
                }
            }
        }

        private  readonly  List<ProductionRule> _productionRules = new List<ProductionRule>();

        public IEnumerable<GrammarElement> Terminals
        {
            get
            {
                foreach (var productionRule in _productionRules)
                {
                    foreach (var terminal in productionRule.Terminals)
                    {
                        yield return terminal;
                    }
                }
            }
        }
        
        public IEnumerable<ProductionRule> ProductionRules => _productionRules;
    }
}
