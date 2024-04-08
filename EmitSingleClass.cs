namespace MongoHelpersGenerator;
internal class EmitSingleClass(ImmutableArray<FirstInformation> list, Compilation compilation, SourceProductionContext context)
{
    public void Emit()
    {
        foreach (var item in list)
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
            compilation.StartSingleUp(item);
            w.CreateSinglePrivateHelpersClass()
            .PopulateSingleImplementCollection()
            .PopulateSingleGetCollection()
            .PopulateSingleGetFilteredList()
            .PopulateSingleGetAll()
            .PopulateSingleRecordMethods()
            .PopulateSingleRedoCollection()
            .PopulateSingleRecordById();
        });
        context.AddSource($"{item.MainSymbol!.Name}.DataAccess.g", builder.ToString());
    }
}