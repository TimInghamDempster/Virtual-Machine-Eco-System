using EBNFParser;

namespace EBNFParserTests
{
    public class CompositionRoot : EBNFParser.CompositionRoot
    {
        public CompositionRoot() : base(true)
        {
            Logger = new TestLogger();
        }
    }
}