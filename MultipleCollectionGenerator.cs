namespace MongoHelpersGenerator;
[Generator]
internal class MultipleCollectionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //step 1
        IncrementalValuesProvider<CustomSymbol> declares1 = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => IsSyntaxTarget(s),
            (t, _) => GetTarget(t))
            .Where(m => m != null)!;


        //step 2
        var declares2 = context.CompilationProvider.Combine(declares1.Collect());

        //step 3
        var declares3 = declares2.SelectMany(static (x, _) =>
        {
            ImmutableHashSet<CustomSymbol> start = [.. x.Right];
            return GetResults(start, x.Left);
        });
        //step 4
        var declares4 = context.CompilationProvider.Combine(declares3.Collect());
        context.RegisterSourceOutput(declares4, (spc, source) =>
        {
            Execute(source.Left, source.Right, spc);
        });
    }
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {
        return syntax is ClassDeclarationSyntax;
    }
    private static ImmutableHashSet<FirstInformation> GetResults(ImmutableHashSet<CustomSymbol> others, Compilation compilation)
    {
        ParserClass parses = new(compilation);
        var results = parses.GetMultipleCollectionResults(others);
        return [.. results];
    }
    private CustomSymbol? GetTarget(GeneratorSyntaxContext context)
    {
        var ourClass = context.GetClassNode(); //can use the sematic model at this stage
        var symbol = context.GetClassSymbol(ourClass);
        if (symbol.Implements("INoSqlDatabaseMultipleCollection"))
        {
            CustomSymbol output = new(ourClass.IsPartial(), symbol, ourClass);
            return output;
        }
        return null;
    }

    private void Execute(Compilation compilation, ImmutableArray<FirstInformation> list, SourceProductionContext context)
    {
        EmitMapClass emitmap = new(list, compilation, context);
        emitmap.Emit();
        EmitMultipleClass emitsfinal = new(list, compilation, context);
        emitsfinal.Emit();
    }
}