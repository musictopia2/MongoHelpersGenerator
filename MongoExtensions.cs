namespace MongoHelpersGenerator;
internal static class MongoExtensions
{
    public static IWriter PopulateMongoDriver(this IWriter w)
    {
        w.GlobalWrite()
            .Write("MongoDB.Driver.");
        return w;
    }
    public static IWriter PopulateMongoCollectionExtensions(this IWriter w)
    {
        w.PopulateMongoDriver()
            .Write("IMongoCollectionExtensions.");
        return w;
    }
    public static IWriter PopulateMongoCursorExtensions(this IWriter w)
    {
        w.PopulateMongoDriver()
            .Write("IAsyncCursorExtensions")
            .Write(".");
        return w;
    }
}