namespace MongoHelpersGenerator;
[Generator]
public class IdGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<CustomSymbol> declares = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => IsSyntaxTarget(s),
            (t, _) => GetTarget(t))
            .Where(m => m != null)!;
        IncrementalValueProvider<(Compilation, ImmutableArray<CustomSymbol>)> compilation
            = context.CompilationProvider.Combine(declares.Collect());
        context.RegisterSourceOutput(compilation, (spc, source) =>
        {
            Execute(source.Item2, spc);
        });
    }
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {
        return syntax is ClassDeclarationSyntax ctx &&
            ctx.IsPublic() &&
            ctx.AttributeLists.Count > 0;
    }
    private CustomSymbol? GetTarget(GeneratorSyntaxContext context)
    {
        var ourClass = context.GetClassNode(); //can use the sematic model at this stage
        var symbol = context.GetClassSymbol(ourClass);
        if (symbol.HasAttribute(ParserClass.MongoAttribute))
        {
            CustomSymbol output = new(ourClass.IsPartial(), symbol, ourClass);
            return output;
        }
        return null;
    }
    private void Execute(ImmutableArray<CustomSymbol> list, SourceProductionContext context)
    {
        try
        {
            foreach (var item in list)
            {
                if (item.PartialClass == false)
                {
                    context.ReportPartialClassRequired(item.Symbol);
                    continue;
                }
                SourceCodeStringBuilder builder = new();
                builder.StartPartialClassImplements(item.Symbol, w =>
                {
                    w.Write(" : global::CommonBasicLibraries.NoSqlHelpers.Interfaces.INoSqlModel");
                }, w =>
                {
                    ProcessClass(w, item.Symbol);
                });
                context.AddSource($"{item.Symbol.Name}mongo.g", builder.ToString());
            }
        }
        catch (Exception)
        {

            
        }
        
    }
    private void ProcessClass(ICodeBlock w, INamedTypeSymbol symbol)
    {
        w.WriteLine("[global::MongoDB.Bson.Serialization.Attributes.BsonId]")
            .WriteLine("[global::MongoDB.Bson.Serialization.Attributes.BsonRepresentation(global::MongoDB.Bson.BsonType.ObjectId)]")
            .WriteLine(w =>
            {
                w.Write("public string Id { get; set; } = ")
                .EndsWithEmptyString();
            })
            .WriteLine(w =>
            {
                symbol.TryGetAttribute(ParserClass.MongoAttribute, out var attributes);
                AttributeProperty property = new("CollectionName", 0);
                string name = attributes.AttributePropertyValue<string>(property)!;
                w.Write("string global::CommonBasicLibraries.NoSqlHelpers.Interfaces.INoSqlModel.CollectionName => ")
                .AppendDoubleQuote(name)
                .Write(";");
            });
    }
}