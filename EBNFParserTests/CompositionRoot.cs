using EBNFParser;

namespace EBNFParserTests
{
    public class CompositionRoot : EBNFParser.CompositionRoot
    {
        public CompositionRoot()
        {
            Logger = new TestLogger();
        }
    }
}