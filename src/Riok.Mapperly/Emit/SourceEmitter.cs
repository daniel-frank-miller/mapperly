using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class SourceEmitter
{
    public static CompilationUnitSyntax Build(MapperDescriptor descriptor)
    {
        var sourceEmitterContext = new SourceEmitterContext(descriptor.Symbol.IsStatic, descriptor.NameBuilder);
        MemberDeclarationSyntax member = ClassDeclaration(descriptor.Syntax.Identifier)
            .WithModifiers(descriptor.Syntax.Modifiers)
            .WithMembers(List(BuildMembers(descriptor, sourceEmitterContext)));

        member = WrapInClassesAsNeeded(descriptor.Symbol, member);
        member = WrapInNamespaceIfNeeded(descriptor.Namespace, member);

        return CompilationUnit()
            .WithMembers(SingletonList(member))
            .WithLeadingTrivia(Nullable(true))
            .NormalizeWhitespace();
    }

    private static IEnumerable<MemberDeclarationSyntax> BuildMembers(
        MapperDescriptor descriptor,
        SourceEmitterContext sourceEmitterContext)
    {
        return descriptor.MethodTypeMappings.Select(mapping => mapping.BuildMethod(sourceEmitterContext));
    }

    private static MemberDeclarationSyntax WrapInClassesAsNeeded(
        INamedTypeSymbol symbol,
        MemberDeclarationSyntax syntax)
    {
        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            if (containingType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not ClassDeclarationSyntax containingTypeSyntax)
                return syntax;

            syntax = containingTypeSyntax.WithMembers(SingletonList(syntax));
            containingType = containingType.ContainingType;
        }

        return syntax;
    }

    private static MemberDeclarationSyntax WrapInNamespaceIfNeeded(string? namespaceName, MemberDeclarationSyntax classDeclaration)
    {
        return namespaceName == null
            ? classDeclaration
            : Namespace(namespaceName).WithMembers(SingletonList(classDeclaration));
    }
}
