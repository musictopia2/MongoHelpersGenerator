namespace MongoHelpersGenerator;
internal static class DataSingleAccessExtensions
{
    private static Compilation? _compilation;
    private static FirstInformation? _item;
    public static void StartSingleUp(this Compilation compilation, FirstInformation item)
    {
        _compilation = compilation;
        _item = item;
    }
    public static ICodeBlock CreateSinglePrivateHelpersClass(this ICodeBlock w)
    {
        w.WriteLine("private static class PrivateHelpers")
            .WriteCodeBlock(w =>
            {
                w.PopulateSingleModelMethod("GetDatabaseName", "DatabaseName")
                .PopulateSingleModelMethod("GetCollectionName", "CollectionName")
                .PopulateConnectionString();
            });
        return w;
    }
    private static ICodeBlock PopulateConnectionString(this ICodeBlock w)
    {
        if (_item!.HasConnectionString)
        {
            w.WriteLine("public static string GetConnectionString(INoSqlConnection connection)")
                .WriteCodeBlock(w =>
                {
                    w.WriteLine("return connection.ConnectionString;");
                });
        }
        return w;
    }
    private static ICodeBlock PopulateSingleModelMethod(this ICodeBlock w, string methodName, string interfaceName)
    {
        w.WriteLine(w =>
        {
            w.Write("public static string ")
            .Write(methodName)
            .Write("(INoSqlDatabaseSingleCollection<")
            .SymbolFullNameWrite(_item!.SingleCollection.Symbol!)
            .Write("> model)");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine(w =>
            {
                w.Write("return model.")
                .Write(interfaceName)
                .Write(";");
            });
        });
        return w;
    }
    public static ICodeBlock PopulateSingleImplementCollection(this ICodeBlock w)
    {
        if (_item!.SingleCollection.NeedsCollectionCode == false)
        {
            return w;
        }
        if (_item.SingleCollection.Catgegory == EnumModelCategory.Attribute)
        {
            w.WriteLine(w =>
            {
                w.PopulateStartInterfaceCollectionInfo(_item!.SingleCollection.Symbol!)
                .Write(" => ");
                _item.SingleCollection.Symbol!.TryGetAttribute(ParserClass.MongoAttribute, out var attributes);
                AttributeProperty property = new("CollectionName", 0);
                string name = attributes.AttributePropertyValue<string>(property)!;
                w.AppendDoubleQuote(name)
                .Write(";");
            });
        }
        else
        {
            w.PopulateInterfaceCollectionInfo(_item!.SingleCollection.Symbol!);
        }
        return w;
    }
    public static ICodeBlock PopulateSingleGetCollection(this ICodeBlock w)
    {
        w.GetCollection(_compilation!, _item!, _item!.SingleCollection, EnumDatabaseCategory.SingleCollection);
        return w;
    }
    public static ICodeBlock PopulateSingleGetAll(this ICodeBlock w)
    {
        w.WriteLine(w =>
        {
            w.Write("internal Task<")
            .BasicListWrite()
            .Write("<")
            .SymbolFullNameWrite(_item!.SingleCollection.Symbol!)
            .Write(">> GetAllAsync()");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine("return GetFilteredListAsync(_ => true);");
        });
        return w;
    }
    public static ICodeBlock PopulateSingleGetFilteredList(this ICodeBlock w)
    {
        w.WriteLine(w =>
        {
            w.Write("private async Task<")
            .BasicListWrite()
            .Write("<")
            .SymbolFullNameWrite(_item!.SingleCollection.Symbol!)
            .Write(">> GetFilteredListAsync(System.Linq.Expressions.Expression<Func<")
            .PopulateModel(_compilation!, _item!.SingleCollection)
            .Write(", bool>> expression)");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine("var firsts = GetCollection();")
            .WriteLine(w =>
            {
                w.Write("var results = await ")
                .PopulateMongoCollectionExtensions()
                .Write("FindAsync(firsts, expression);");
            })
            .WriteLine(w =>
            {
                w.BasicListWrite()
                .Write("<")
                .SymbolFullNameWrite(_item!.SingleCollection.Symbol!)
                .Write("> output = new();");
            })
            .WriteLine(w =>
            {
                w.Write("var nexts = ")
                .PopulateMongoCursorExtensions()
                .Write("ToList(results);");
            })
            .WriteLine("foreach (var item in nexts)")
            .WriteCodeBlock(w =>
            {
                if (_item!.SingleCollection.Catgegory == EnumModelCategory.None)
                {
                    w.WriteLine("output.Add(item.MapMongo());");
                }
                else
                {
                    w.WriteLine("output.Add(item);");
                }
            })
            .WriteLine("return output;");
        });
        return w;
    }
    private static ICodeBlock WriteFilter(this ICodeBlock w, string extras)
    {
        w.WriteLine(w =>
        {
            w.Write("var filters = ")
            .PopulateMongoDriver()
            .Write("Builders<")
            .PopulateModel(_compilation!, _item!.SingleCollection)
            .Write(">.Filter.Eq(")
            .AppendDoubleQuote("id")
            .Write(", ")
            .Write(extras)
            .Write(");");
        });
        return w;
    }
    //await DeleteRecordAsync(record);
    //await InsertRecordAsync(record);
    public static ICodeBlock PopulateSingleRecordMethods(this ICodeBlock w)
    {
        w.PopulateRecordMethod("InsertRecordAsync", _compilation!, _item!.SingleCollection, EnumDatabaseCategory.SingleCollection, true, w =>
        {
            w.WriteLine("await firsts.InsertOneAsync(output);")
            .WriteLine("record.Id = output.Id;");
        })
        .PopulateRecordMethod("UpsertRecordAsync", _compilation!, _item!.SingleCollection, EnumDatabaseCategory.SingleCollection, false, w =>
        {
            //w.WriteFilter("output.Id")
            w.WriteLine("await DeleteRecordAsync(record);")
            .WriteLine("await InsertRecordAsync(record);");
            //.WriteLine(w =>
            //{

            //    w.Write("return firsts.ReplaceOneAsync(filters, output, new ")
            //    .PopulateMongoDriver()
            //    .Write("ReplaceOptions { IsUpsert = true });");
            //});
        })
        .PopulateRecordMethod("DeleteRecordAsync", _compilation!, _item!.SingleCollection, EnumDatabaseCategory.SingleCollection, true, w =>
        {
            w.WriteLine(w =>
            {
                w.Write("return ")
                .PopulateMongoCollectionExtensions()
                .Write("DeleteOneAsync(firsts, xx => xx.Id == output.Id);");
            });
        });
        return w;
    }
    public static ICodeBlock PopulateSingleRedoCollection(this ICodeBlock w)
    {
        w.WriteLine("internal async Task RedoCollectionAsync()")
        .WriteCodeBlock(w =>
        {
            w.WriteDbCode(_item!)
            .WriteLine("await db.DropCollectionAsync(PrivateHelpers.GetCollectionName(this));")
            .WriteLine("await db.CreateCollectionAsync(PrivateHelpers.GetCollectionName(this));");
        });
        return w;
    }
    public static ICodeBlock PopulateSingleRecordById(this ICodeBlock w)
    {
        w.WriteLine(w =>
        {
            w.Write("internal async Task<")
            .SymbolFullNameWrite(_item!.SingleCollection.Symbol!)
            .Write("> GetRecordByIdAsync(string id)");
        })
        .WriteCodeBlock(w =>
        {
            w.WriteLine("var firsts = GetCollection();")
            .WriteFilter("id")
            .WriteLine(w =>
            {
                w.Write("var results = await ")
                .PopulateMongoCollectionExtensions()
                .Write("FindAsync(firsts, filters);");
            })
            .WriteLine(w =>
            {
                w.Write("var item = await ")
                .PopulateMongoCursorExtensions()
                .Write("FirstAsync(results);");
            });
            if (_item!.SingleCollection.Catgegory == EnumModelCategory.None)
            {
                w.WriteLine("var output = item.MapMongo();");
            }
            else
            {
                w.WriteLine("var output = item;");
            }
            w.WriteLine("return output;");
        });
        return w;
    }
}