namespace MongoHelpersGenerator;
internal static class DataMultiAccessExtensions
{
    private static Compilation? _compilation;
    private static FirstInformation? _item;
    public static void StartMultiUp(this Compilation compilation, FirstInformation item)
    {
        _compilation = compilation;
        _item = item;
    }
    private static IWriter PopulateStartCollectionName(this IWriter w, CollectionInfo collection)
    {
        w.Write("public static string Get")
            .Write(collection.Symbol!.Name)
            .Write("CollectionName");
        return w;
    }
    public static ICodeBlock CreateMultiPrivateHelpersClass(this ICodeBlock w)
    {
        w.WriteLine("private static class PrivateHelpers")
            .WriteCodeBlock(w =>
            {
                w.PopulateModelMethod("GetDatabaseName", "DatabaseName")
                .PopulateConnectionString();
                foreach (var c in _item!.Collections)
                {
                    w.WriteLine(w =>
                    {
                        w.PopulateStartCollectionName(c);
                        if (c.Catgegory != EnumModelCategory.Interface)
                        {
                            string name;
                            if (c.Catgegory == EnumModelCategory.None)
                            {
                                name = c.Name;
                            }
                            else
                            {
                                AttributeProperty property = new("CollectionName", 0);
                                c.Symbol!.TryGetAttribute(ParserClass.MongoAttribute, out var attributes);
                                name = attributes.AttributePropertyValue<string>(property)!;
                            }
                            w.Write(" => ")
                            .AppendDoubleQuote(name)
                            .Write(";");
                        }
                    });
                    if (c.Catgegory == EnumModelCategory.Interface)
                    {
                        w.WriteCodeBlock(w =>
                        {
                            w.WriteLine("get")
                            .WriteCodeBlock(w =>
                            {
                                w.WriteLine(w =>
                                {
                                    w.Write("INoSqlModel temps = new ")
                                    .SymbolFullNameWrite(c.Symbol!)
                                    .Write("();");
                                })
                                .WriteLine("return temps.CollectionName;");
                            });
                        });
                    }
                }


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
    private static ICodeBlock PopulateModelMethod(this ICodeBlock w, string methodName, string interfaceName)
    {
        w.WriteLine(w =>
        {
            w.Write("public static string ")
            .Write(methodName)
            .Write("(INoSqlDatabaseMultipleCollection model)");
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
    public static ICodeBlock PopulateMultiGetCollection(this ICodeBlock w)
    {
        foreach (var c in _item!.Collections)
        {
            w.GetCollection(_compilation!, _item, c, EnumDatabaseCategory.ManyCollections);
        }
        return w;
    }
    internal static ICodeBlock PopulateFirstGetCollection(this ICodeBlock w, INamedTypeSymbol symbol)
    {
        w.WriteLine(w =>
        {
            w.Write("var firsts = Get")
            .Write(symbol.Name)
            .Write("Collection();");
        });
        return w;
    }
    public static ICodeBlock PopulateMultiGetFilteredList(this ICodeBlock w)
    {
        foreach (var c in _item!.Collections)
        {
            w.WriteLine(w =>
            {
                w.Write("private async Task<")
                .BasicListWrite()
                .Write("<")
                .SymbolFullNameWrite(c.Symbol!)
                .Write(">> GetFilteredListAsync(System.Linq.Expressions.Expression<Func<")
                .PopulateModel(_compilation!, c)
                .Write(", bool>> expression)");
            })
            .WriteCodeBlock(w =>
            {
                w.PopulateFirstGetCollection(c.Symbol!)
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
                    .SymbolFullNameWrite(c.Symbol!)
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
                    if (c.Catgegory == EnumModelCategory.None)
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
        }
        return w;
    }
    public static ICodeBlock PopulateMultiGetAll(this ICodeBlock w)
    {
        foreach (var c in _item!.Collections)
        {
            w.WriteLine(w =>
            {
                w.Write("internal async Task<")
                .BasicListWrite()
                .Write("<")
                .SymbolFullNameWrite(c!.Symbol!)
                .Write(">> GetAll");
                string name = c.Name;
                name = name.Replace("Model", "s");
                w.Write(name)
                .Write("Async()");
            })
            .WriteCodeBlock(w =>
            {
                w.PopulateFirstGetCollection(c.Symbol!)
                .WriteLine(w =>
                {
                    w.Write("var results = await ")
                    .PopulateMongoCollectionExtensions()
                    .Write("FindAsync(firsts,  _ => true);");
                })
                .WriteLine(w =>
                {
                    w.BasicListWrite()
                    .Write("<")
                    .SymbolFullNameWrite(c.Symbol!)
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
                    if (c.Catgegory == EnumModelCategory.None)
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
        }
        return w;
    }
    private static ICodeBlock WriteFilter(this ICodeBlock w, CollectionInfo collection, string extras)
    {
        w.WriteLine(w =>
        {
            w.Write("var filters = ")
            .PopulateMongoDriver()
            .Write("Builders<")
            .PopulateModel(_compilation!, collection)
            .Write(">.Filter.Eq(")
            .AppendDoubleQuote("id")
            .Write(", ")
            .Write(extras)
            .Write(");");
        });
        return w;
    }
    public static ICodeBlock PopulateMultiRecordMethods(this ICodeBlock w)
    {
        foreach (var c in _item!.Collections)
        {
            w.PopulateRecordMethod("InsertRecordAsync", _compilation!, c!, EnumDatabaseCategory.ManyCollections, true, w =>
            {
                w.WriteLine("return firsts.InsertOneAsync(output);");
            })
           .PopulateRecordMethod("UpsertRecordAsync", _compilation!, c!, EnumDatabaseCategory.ManyCollections, false, w =>
           {
               //w.WriteFilter(c!, "output.Id")
              w.WriteLine("await DeleteRecordAsync(record);")
              .WriteLine("await InsertRecordAsync(record);");
               //.WriteLine(w =>
               //{

               //    w.Write("return firsts.ReplaceOneAsync(filters, output, new ")
               //    .PopulateMongoDriver()
               //    .Write("ReplaceOptions { IsUpsert = true });");
               //});
           })
           .PopulateRecordMethod("DeleteRecordAsync", _compilation!, c!, EnumDatabaseCategory.ManyCollections, true, w =>
           {
               w.WriteLine(w =>
               {
                   w.Write("return ")
                   .PopulateMongoCollectionExtensions()
                   .Write("DeleteOneAsync(firsts, xx => xx.Id == output.Id);");
               });
           });
        }
        return w;
    }
    public static ICodeBlock PopulateMultiRedoCollection(this ICodeBlock w)
    {
        foreach (var c in _item!.Collections)
        {
            w.WriteLine(w =>
            {
                w.Write("internal async Task Redo")
                .Write(c.Symbol!.Name)
                .Write("CollectionAsync()");
            })
           .WriteCodeBlock(w =>
           {
               w.WriteDbCode(_item!)
               .WriteLine(w =>
               {
                   w.Write("await db.DropCollectionAsync(")
                   .PopulateGetCollectionName(c.Symbol!);
               })
               .WriteLine(w =>
               {
                   w.Write("await db.CreateCollectionAsync(")
                   .PopulateGetCollectionName(c.Symbol!);
               });
           });
        }
        return w;
    }
    private static IWriter PopulateGetCollectionName(this IWriter w, INamedTypeSymbol symbol)
    {
        w.Write("PrivateHelpers.Get")
            .Write(symbol.Name)
            .Write("CollectionName);");
        return w;
    }
    public static ICodeBlock PopulateMultiRecordById(this ICodeBlock w)
    {
        foreach (var c in _item!.Collections)
        {
            w.WriteLine(w =>
            {
                w.Write("internal async Task<")
                .SymbolFullNameWrite(c.Symbol!);
                string name = c.Symbol!.Name.Replace("Model", "s");
                w.Write(">Get")
                .Write(name)
                .Write("RecordByIdAsync(string id)");
            })
            .WriteCodeBlock(w =>
            {
                w.PopulateFirstGetCollection(c.Symbol!)
                .WriteFilter(c, "id")
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
                if (c!.Catgegory == EnumModelCategory.None)
                {
                    w.WriteLine("var output = item.MapMongo();");
                }
                else
                {
                    w.WriteLine("var output = item;");
                }
                w.WriteLine("return output;");
            });
        }
        return w;
    }
}