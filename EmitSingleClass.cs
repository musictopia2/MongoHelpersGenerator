namespace MongoHelpersGenerator;
internal class EmitSingleClass
{
    private readonly BasicList<FirstInformation> _list;
    private readonly Compilation _compilation;
    private readonly SourceProductionContext _context;
    public EmitSingleClass(BasicList<FirstInformation> list, Compilation compilation, SourceProductionContext context)
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
            _compilation.StartSingleUp(item);
            w.CreateSinglePrivateHelpersClass()
            .PopulateSingleImplementCollection()
            .PopulateSingleGetCollection()
            .PopulateSingleGetFilteredList()
            .PopulateSingleGetAll()
            .PopulateSingleRecordMethods()
            .PopulateSingleRedoCollection()
            .PopulateSingleRecordById();
        });
        _context.AddSource($"{item.MainSymbol!.Name}.DataAccess.g", builder.ToString());
    }
}