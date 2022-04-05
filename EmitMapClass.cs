namespace MongoHelpersGenerator;
internal class EmitMapClass
{
    private readonly BasicList<FirstInformation> _list;
    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    public EmitMapClass(BasicList<FirstInformation> list, Compilation compilation, SourceProductionContext context)
    {
        _list = list;
        _compilation = compilation;
        _context = context;
    }
    public void Emit()
    {
        foreach (var item in _list)
        {
            if (item.HasPartial == false)
            {
                _context.ReportPartialClassRequired(item.MainSymbol!);
                continue;
            }
            foreach (var c in item.Collections)
            {
                if (c.Catgegory == EnumModelCategory.None)
                {
                    SourceCodeStringBuilder builder = new();
                    builder.StartCreatingNewModel(_compilation, c.Symbol!, w =>
                    {
                        FinishModel(w, c.Symbol!);
                    });
                    _context.AddSource($"{c.Symbol!.Name}.Mongo.g", builder.ToString());
                }
            }
        }
        MappingExtensions();

    }
    private void MappingExtensions()
    {
        SourceCodeStringBuilder builder = new();
        builder.StartInternalGlobalProcesses(_compilation, "MongoMappingExtensions", w =>
        {
            foreach (var item in _list)
            {
                if (item.HasPartial == false)
                {
                    continue;
                }
                foreach (var c in item.Collections)
                {
                    if (c.Catgegory == EnumModelCategory.None)
                    {
                        MapFrom(w, c.Symbol!);
                        MapTo(w, c.Symbol!);
                        MapList(w, c.Symbol!);
                    }
                }
            }
        });
        _context.AddSource("mongomappings.g", builder.ToString());
    }
    private void MapList(ICodeBlock w, INamedTypeSymbol symbol)
    {
        w.WriteLine(w =>
        {
            w.Write("public static ")
            .BasicListWrite()
            .Write("<")
            .SymbolFullNameWrite(symbol)
            .Write("> MapMongo(this ")
            .BasicListWrite()
            .Write("<")
            .PopulateMongoModel(_compilation, symbol.Name)
            .Write("> list)");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine(w =>
            {
                w.BasicListWrite()
                .Write("<")
                .SymbolFullNameWrite(symbol)
                .Write("> output = new();");
            })
            .WriteLine("foreach (var item in list)")
            .WriteCodeBlock(w =>
            {
                w.WriteLine("output.Add(item.MapMongo());");
            })
            .WriteLine("return output;");
        });
    }
    private void MapFrom(ICodeBlock w, INamedTypeSymbol symbol)
    {
        w.WriteLine(w =>
        {
            w.Write("public static ")
            .PopulateMongoModel(_compilation, symbol.Name)
            .Write(" MapMongo(this ")
            .SymbolFullNameWrite(symbol)
            .Write(" model)");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine(w =>
            {
                w.PopulateMongoModel(_compilation, symbol.Name)
                .Write(" output = new();");
            });
            FinishMapping(w, symbol);
            w.WriteLine("return output;");
        });
    }
    private void MapTo(ICodeBlock w, INamedTypeSymbol symbol)
    {
        w.WriteLine(w =>
        {
            w.Write("public static ")
            .SymbolFullNameWrite(symbol)
            .Write(" MapMongo(this ")
            .PopulateMongoModel(_compilation, symbol.Name)
            .Write(" model)");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine(w =>
            {
                w.SymbolFullNameWrite(symbol)
                .Write(" output = new();");
            });
            FinishMapping(w, symbol);
            w.WriteLine("return output;");
        });
    }
    private void FinishMapping(ICodeBlock w, INamedTypeSymbol symbol)
    {
        var list = symbol.GetAllPublicProperties();
        list.RemoveAllOnly(x => x.IsReadOnly);
        foreach (var item in list)
        {
            w.WriteLine(w =>
            {
                w.Write("output.")
                .Write(item.Name)
                .Write(" = ")
                .Write("model.")
                .Write(item.Name)
                .Write(";");
            });
        }
    }
    private void FinishModel(ICodeBlock w, INamedTypeSymbol symbol)
    {
        w.PopulateMongoId()
            .PopulateCollection();
        var list = symbol.GetAllPublicProperties();
        list.RemoveAllOnly(x => x.Name == "Id"); //because already handled.
        list.RemoveAllOnly(x => x.IsReadOnly);
        foreach (var property in list)
        {
            w.WriteLine(w =>
            {
                if (property.Type.Name.ToLower() == "string")
                {
                    w.Write("public string ")
                    .Write(property.Name)
                    .Write(" { get; set; } = ")
                    .EndsWithEmptyString();
                }
                else if (property.Type.Name.StartsWith("Nullable"))
                {
                    w.Write("public Nullable<")
                    .Write(property.Type.GetSingleGenericTypeUsed()!)
                    .Write("> ")
                    .Write(property.Name)
                     .Write(" { get; set; }");
                }
                else if (property.Type.IsSimpleType())
                {
                    w.Write("public ")
                   .Write(property.Type.Name)
                   .Write(" ")
                   .Write(property.Name)
                   .Write(" { get; set; }");
                }
                else
                {
                    w.Write("public ")
                    .SymbolFullNameWrite((INamedTypeSymbol)property.Type)
                    .Write("? ")
                    .Write(property.Name)
                    .Write(" { get; set; }");
                }
            });
        }
    }
}