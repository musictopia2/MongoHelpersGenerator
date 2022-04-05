namespace MongoHelpersGenerator;
internal class CollectionInfo
{
    public INamedTypeSymbol? Symbol { get; set; }
    public EnumModelCategory Catgegory { get; set; } //the name could be found from other places (if not given otherwise).
    public string Name { get; set; } = "";
    public bool NeedsCollectionCode { get; set; }
}