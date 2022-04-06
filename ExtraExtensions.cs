namespace MongoHelpersGenerator;
internal static class ExtraExtensions
{
    public static void StartCreatingNewModel(this SourceCodeStringBuilder builder, Compilation compilation, INamedTypeSymbol oldModel, Action<ICodeBlock> action)
    {
        string ns = compilation.AssemblyName!;
        builder.WriteLine("#nullable enable")
        .WriteLine(w =>
        {
            w.Write("namespace ")
            .Write(ns)
            .Write(".MongoModels")
            .Write(";");
        })
        .WriteLine(w =>
        {
            w.Write("internal class ")
            .Write(oldModel.Name)
            .Write(" : global::CommonBasicLibraries.NoSqlHelpers.Interfaces.INoSqlModel");
        })
        .WriteCodeBlock(w =>
        {
            action.Invoke(w);
        });
    }
    public static ICodeBlock PopulateMongoId(this ICodeBlock w)
    {
        w.WriteLine("[global::MongoDB.Bson.Serialization.Attributes.BsonId]")
            .WriteLine("[global::MongoDB.Bson.Serialization.Attributes.BsonRepresentation(global::MongoDB.Bson.BsonType.ObjectId)]")
            .WriteLine(w =>
            {
                w.Write("public string Id { get; set; } = ")
                .EndsWithEmptyString();
            });
        return w;
    }
    public static ICodeBlock PopulateCollection(this ICodeBlock w)
    {
        w.WriteLine(w =>
        {
            w.Write("string global::CommonBasicLibraries.NoSqlHelpers.Interfaces.INoSqlModel.CollectionName => ")
                .CustomExceptionLine(w =>
                {
                    w.Write("Since this is being used from a class that only has one collection, use INoSqlDatabaseSingleCollection Instead");
                });
        });
        return w;
    }
    public static IWriter PopulateMongoModel(this IWriter w, Compilation compilation, string name)
    {
        w.GlobalWrite()
            .Write(compilation.AssemblyName!)
            .Write(".MongoModels.")
            .Write(name);
        return w;
    }
    public static IWriter PopulateModel(this IWriter w, Compilation compilation, CollectionInfo information)
    {
        if (information.Catgegory == EnumModelCategory.None)
        {
            w.PopulateMongoModel(compilation, information.Symbol!.Name);
        }
        else
        {
            w.SymbolFullNameWrite(information.Symbol!);
        }
        return w;
    }
    public static IWriter PopulateList(this IWriter w, CollectionInfo information)
    {
        w.BasicListWrite()
            .Write("<")
            .SymbolFullNameWrite(information.Symbol!)
            .Write(">");
        return w;
    }
    private static IWriter PopulateRecord(this IWriter w, CollectionInfo information)
    {
        w.Write("(")
            .SymbolFullNameWrite(information.Symbol!)
            .Write(" record)");
        return w;
    }
    public static ICodeBlock PopulateRecordMethod(this ICodeBlock w, string methodName, Compilation compilation, CollectionInfo information, EnumDatabaseCategory category, bool needsFirst, Action<ICodeBlock> action)
    {
        w.WriteLine(w =>
        {
            if (needsFirst == false)
            {
                w.Write("internal async Task ");
            }
            else
            {
                w.Write("internal Task ");
            }
            w.Write(methodName)
            .PopulateRecord(information);
        })
        .WriteCodeBlock(w =>
        {
            if (needsFirst)
            {
                w.MapMongo(compilation, information, category);
            }
            action.Invoke(w);
        });
        return w;
    }
    private static ICodeBlock MapMongo(this ICodeBlock w, Compilation compilation, CollectionInfo information, EnumDatabaseCategory category)
    {
        if (category == EnumDatabaseCategory.ManyCollections)
        {
            w.PopulateFirstGetCollection(information.Symbol!);
        }
        else
        {
            w.WriteLine("var firsts = GetCollection();");
        }
        if (information.Catgegory == EnumModelCategory.None)
        {
            w.WriteLine(w =>
            {
                w.PopulateModel(compilation, information)
                .Write(" output = record.MapMongo();");
            });
        }
        else
        {
            w.WriteLine("var output = record;"); //to have more consistency.  sometimes, i don't need mappings.
        }
        return w;
    }
}