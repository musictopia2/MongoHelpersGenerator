namespace MongoHelpersGenerator;
internal class EmitMultipleClass
{
    private readonly BasicList<FirstInformation> _list;
    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    public EmitMultipleClass(BasicList<FirstInformation> list, Compilation compilation, SourceProductionContext context)
    {
        _list = list;
        _compilation = compilation;
        _context = context;
    }
    public void Emit()
    {
        foreach (var item in _list)
        {
            if (item.HasPartial == false || item.HasId == false)
            {
                continue; //because already handled.
            }
            CreateDataAccess(item);
        }
    }
    private void CreateDataAccess(FirstInformation item)
    {
        SourceCodeStringBuilder builder = new();
        builder.StartPartialClass(item.MainSymbol!, w =>
        {
            _compilation.StartMultiUp(item);
            w.CreateMultiPrivateHelpersClass()
            .PopulateMultiGetCollection()
            .PopulateMultiGetFilteredList()
            .PopulateMultiGetAll()
            .PopulateMultiRecordMethods()
            .PopulateMultiRedoCollection()
            .PopulateMultiRecordById();
        });
        _context.AddSource($"{item.MainSymbol!.Name}.DataAccess.g", builder.ToString());
    }
}