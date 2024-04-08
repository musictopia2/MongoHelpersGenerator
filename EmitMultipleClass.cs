namespace MongoHelpersGenerator;
internal class EmitMultipleClass(ImmutableArray<FirstInformation> list, Compilation compilation, SourceProductionContext context)
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
            compilation.StartMultiUp(item);
            w.CreateMultiPrivateHelpersClass()
            .PopulateMultiGetCollection()
            .PopulateMultiGetFilteredList()
            .PopulateMultiGetAll()
            .PopulateMultiRecordMethods()
            .PopulateMultiRedoCollection()
            .PopulateMultiRecordById();
        });
        context.AddSource($"{item.MainSymbol!.Name}.DataAccess.g", builder.ToString());
    }
}