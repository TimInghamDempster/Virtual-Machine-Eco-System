using System.Collections.Generic;

namespace EBNFParser
{
    public enum ElementType
    {
        Terminal,
        Rule,
        Repeat
    }

    public class ElementComparer : IEqualityComparer<GrammarElement>
    {
        public bool Equals(GrammarElement x, GrammarElement y)
        {
            return (x.Name == y.Name) && (x.Type == y.Type);
        }

        public int GetHashCode(GrammarElement elm)
        {
            return elm.Name.GetHashCode() ^ elm.Type.GetHashCode();
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