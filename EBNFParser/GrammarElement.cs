namespace EBNFParser
{
    public enum ElementType
    {
        Terminal,
        Rule,
        Repeat
    }

    public class GrammarElement
    {
        public Pattern SubPattern { get; }
        public string Name { get; }

        public bool IsTerminal => Type == ElementType.Terminal;
        public bool IsRule => Type == ElementType.Rule;
        public bool IsRepeat => Type == ElementType.Repeat;

        public ElementType Type {get;}

        public GrammarElement(string name, ElementType type, Pattern subPattern = null)
        {
            Name = name;
            Type = type;
            SubPattern = subPattern;
        }
    }
}