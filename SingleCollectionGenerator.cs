﻿namespace MongoHelpersGenerator;
[Generator]
internal class SingleCollectionGenerator : IIncrementalGenerator
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
            Execute(source.Item1, source.Item2, spc);
        });
    }
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {
        return syntax is ClassDeclarationSyntax;
    }
    private CustomSymbol? GetTarget(GeneratorSyntaxContext context)
    {
        var ourClass = context.GetClassNode(); //can use the sematic model at this stage
        var symbol = context.GetClassSymbol(ourClass);
        if (symbol.Implements("INoSqlDatabaseSingleCollection"))
        {
            CustomSymbol output = new(ourClass.IsPartial(), symbol, ourClass);
            return output;
        }
        return null;
    }
    private void Execute(Compilation compilation, ImmutableArray<CustomSymbol> list, SourceProductionContext context)
    {
        
        try
        {
            var others = list.Distinct();
            ParserClass parses = new(compilation);
            var results = parses.GetSingleCollectionResults(others);
            EmitMapClass emitmap = new(results, compilation, context);
            emitmap.Emit();
            EmitSingleClass emitsfinal = new(results, compilation, context);
            emitsfinal.Emit();
        }
        catch (Exception)
        {
            //try to make it ignore errors.  so i don't have to reload visual studio as much.

        }

    }
}