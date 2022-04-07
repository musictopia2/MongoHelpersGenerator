namespace MongoHelpersGenerator;
internal class FirstInformation
{
    public INamedTypeSymbol? MainSymbol { get; set; }
    //for now, if there are duplicates, will cause major issues for mappings.
    //if that is an issue, rethink.
    public BasicList<CollectionInfo> Collections { get; set; } = new();
    public bool HasPartial { get; set; }
    public bool HasConnectionString { get; set; }
    public CollectionInfo SingleCollection => Collections.Single(); //if there is not single, will raise error.
    public bool HasId { get; set; }
}