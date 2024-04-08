namespace MongoHelpersGenerator;
internal class ParserClass(Compilation compilation)
{
    public const string MongoAttribute = "UseMongoAttribute";
    public BasicList<FirstInformation> GetSingleCollectionResults(IEnumerable<CustomSymbol> list)
    {
        BasicList<FirstInformation> output = [];
        foreach (CustomSymbol symbol in list)
        {
            output.Add(GetSingleInfo(symbol));
        }
        return output;
    }
    public BasicList<FirstInformation> GetMultipleCollectionResults(IEnumerable<CustomSymbol> list)
    {
        BasicList<FirstInformation> output = [];
        foreach (var item in list)
        {
            output.Add(GetMultipleInfo(item));
        }
        return output;
    }
    private INamedTypeSymbol GetModelSymbol(INamedTypeSymbol symbol)
    {
        foreach (var temp in symbol.AllInterfaces) //try this way (?)
        {
            if (temp.Name == "INoSqlDatabaseSingleCollection")
            {
                return (INamedTypeSymbol)temp.TypeArguments.Single();
            }
        }
        throw new Exception("No model found");
    }
    private bool RequiresCollectionCode(ClassDeclarationSyntax node)
    {
        string payLoad = node.ToString();
        if (payLoad.Contains("public string CollectionName"))
        {
            return false; //because already handled
        }
        return payLoad.Contains(">.CollectionName") == false;
    }
    private FirstInformation GetSingleInfo(CustomSymbol custom)
    {
        FirstInformation output = new();
        output.HasPartial = custom.PartialClass;
        output.MainSymbol = custom.Symbol;
        output.HasConnectionString = custom.Symbol.Implements("INoSqlConnection");
        CollectionInfo other = new();
        other.Symbol = GetModelSymbol(custom.Symbol);
        other.Catgegory = GetModelCategory(other.Symbol);
        var list = other.Symbol.GetAllPublicProperties();
        other.HasId = list.Any(x => x.Name == "Id");
        output.HasId = other.HasId;
        output.Collections.Add(other);
        if (other.Catgegory == EnumModelCategory.None)
        {
            other.NeedsCollectionCode = false;
        }
        else
        {
            other.NeedsCollectionCode = RequiresCollectionCode(custom.Node);
        }
        return output;
    }
    private FirstInformation GetMultipleInfo(CustomSymbol custom)
    {
        FirstInformation output = new();
        ParseContext firstParse = new(compilation, custom.Node);
        output.HasPartial = custom.Node.IsPartial();
        output.HasConnectionString = custom.Symbol.Implements("INoSqlConnection");
        output.MainSymbol = custom.Symbol;
        var makeCalls = ParseUtils.FindCallsOfMethodWithName(firstParse, custom.Node, "Make");
        foreach (var make in makeCalls)
        {
            INamedTypeSymbol makeType = (INamedTypeSymbol)make.MethodSymbol.TypeArguments[0]!;
            CollectionInfo collection = new();
            collection.Symbol = makeType;
            collection.Name = ParseUtils.GetStringContent(make);
            collection.Catgegory = GetModelCategory(makeType);
            var list = collection.Symbol.GetAllPublicProperties();
            collection.HasId = list.Any(x => x.Name == "Id");
            output.Collections.Add(collection);
        }
        output.HasId = output.Collections.All(x => x.HasId);
        return output;
    }
    private EnumModelCategory GetModelCategory(INamedTypeSymbol symbol)
    {
        if (symbol.Implements("INoSqlModel"))
        {
            return EnumModelCategory.Interface;
        }
        if (symbol.HasAttribute(MongoAttribute))
        {
            return EnumModelCategory.Attribute;
        }
        return EnumModelCategory.None;
    }
}