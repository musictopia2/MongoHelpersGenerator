namespace MongoHelpersGenerator;
internal static class CollectionExtensions
{
    public static ICodeBlock PopulateAttributeCollectionInfo(this ICodeBlock w, INamedTypeSymbol modelSymbol)
    {
        modelSymbol.TryGetAttribute(ParserClass.MongoAttribute, out var attributes);
        AttributeProperty property = new("CollectionName", 0);
        string name = attributes.AttributePropertyValue<string>(property)!;
        w.WriteLine(w =>
        {
            w.Write("string INoSqlDatabaseSingleCollection<")
            .SymbolFullNameWrite(modelSymbol)
            .Write(">.CollectionName => ")
            .AppendDoubleQuote(name)
            .Write(";");
        });
        return w;
    }
    public static IWriter PopulateStartInterfaceCollectionInfo(this IWriter w, INamedTypeSymbol modelSymbol)
    {
        w.Write("string INoSqlDatabaseSingleCollection<")
            .SymbolFullNameWrite(modelSymbol)
            .Write(">")
            .Write(".CollectionName");
        return w;
    }
    public static ICodeBlock PopulateInterfaceCollectionInfo(this ICodeBlock w, INamedTypeSymbol modelSymbol)
    {
        w.WriteLine(w =>
        {
            w.PopulateStartInterfaceCollectionInfo(modelSymbol);
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine("get")
            .WriteCodeBlock(w =>
            {
                w.WriteLine(w =>
                {
                    w.Write("INoSqlModel temps = new ")
                    .SymbolFullNameWrite(modelSymbol)
                    .Write("();");
                })
                .WriteLine("return temps.CollectionName;");
            });
        });
        return w;
    }
    private static ICodeBlock GetCollectionBlock(this ICodeBlock w, Compilation compilation, FirstInformation firsts, CollectionInfo collection, EnumDatabaseCategory category)
    {
        w.WriteCodeBlock(w =>
        {
            w.WriteDbCode(firsts)
            .WriteLine(w =>
            {
                if (category == EnumDatabaseCategory.ManyCollections)
                {
                    w.Write("var output = db.GetCollection<")
                   .PopulateModel(compilation, collection)
                   .Write(">(PrivateHelpers.Get")
                   .Write(collection.Symbol!.Name)
                   .Write("CollectionName);");
                }
                else
                {
                    w.Write("var output = db.GetCollection<")
                   .PopulateModel(compilation, collection)
                   .Write(">(PrivateHelpers.GetCollectionName(this));");
                }
            })
            .WriteLine("return output;");
        });
        return w;
    }
    public static ICodeBlock WriteDbCode(this ICodeBlock w, FirstInformation firsts)
    {
        w.WriteLine(w =>
        {
            w.Write("var client = new ")
            .PopulateMongoDriver()
            .Write("MongoClient(");
            if (firsts.HasConnectionString == false)
            {
                w.Write(");");
            }
            else
            {
                w.Write("PrivateHelpers.GetConnectionString(this));");
            }
        })
        .WriteLine("var db = client.GetDatabase(PrivateHelpers.GetDatabaseName(this));");
        return w;
    }
    public static ICodeBlock GetCollection(this ICodeBlock w, Compilation compilation, FirstInformation firsts, CollectionInfo collection, EnumDatabaseCategory category)
    {
        w.WriteLine(w =>
        {
            w.Write("private ")
            .PopulateMongoDriver()
            .Write("IMongoCollection")
            .Write("<")
            .PopulateModel(compilation, collection);
            if (category == EnumDatabaseCategory.SingleCollection)
            {
                w.Write("> GetCollection()");
            }
            else
            {
                w.Write("> Get")
                .Write(collection.Symbol!.Name)
                .Write("Collection()");
            }
        })
        .GetCollectionBlock(compilation, firsts, collection, category);
        return w;
    }
}