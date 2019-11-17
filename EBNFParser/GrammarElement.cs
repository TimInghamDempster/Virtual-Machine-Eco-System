using System.Collections.Generic;

namespace EBNFParser
{
    public enum ElementType
    {
        Terminal,
        Rule,
        Repeat
    }

    public class ElementComparer : IEqualityComparer<(ProductionRule rule, GrammarElement elm)>
    {
        public bool Equals((ProductionRule rule, GrammarElement elm) x, (ProductionRule rule, GrammarElement elm) y)
        {
            return (x.elm.Name == y.elm.Name) && (x.elm.Type == y.elm.Type);
        }

        public int GetHashCode((ProductionRule rule, GrammarElement elm) tuple)
        {
            return tuple.elm.Name.GetHashCode() ^ tuple.elm.Type.GetHashCode();
        }
    }

    public class GrammarElement
    {
        public Pattern? SubPattern { get; }
        public string Name { get; }

        public bool IsTerminal => Type == ElementType.Terminal;
        public bool IsRule => Type == ElementType.Rule;
        public bool IsRepeat => Type == ElementType.Repeat;

        public ElementType Type {get;}

        public GrammarElement(string name, ElementType type, Pattern? subPattern = null)
        {
            Name = name;
            Type = type;
            SubPattern = subPattern;
        }
    }
}