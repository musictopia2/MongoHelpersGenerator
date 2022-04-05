namespace MongoHelpersGenerator;
public record class CustomSymbol(bool PartialClass, INamedTypeSymbol Symbol, ClassDeclarationSyntax Node);