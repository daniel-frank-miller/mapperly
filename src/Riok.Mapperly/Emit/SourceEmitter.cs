using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors;
using Riok.Mapperly.Emit.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Emit;

public static class SourceEmitter
{
    private const string AutoGeneratedComment = "// <auto-generated />";

    public static CompilationUnitSyntax Build(MapperDescriptor descriptor, CancellationToken cancellationToken)
    {
        var ctx = new SourceEmitterContext(descriptor.Static, descriptor.NameBuilder, new SyntaxFactoryHelper());
        ctx = IndentForMapper(ctx, descriptor.Symbol);

        var memberCtx = ctx.AddIndentation();
        var members = BuildMembers(memberCtx, descriptor, cancellationToken);
        members = members.SeparateByLineFeed(memberCtx.SyntaxFactory.Indentation);
        MemberDeclarationSyntax mapperClass = ctx.SyntaxFactory.Class(descriptor.Symbol.Name, descriptor.Syntax.Modifiers, List(members));

        var compilationUnitMembers = new List<MemberDeclarationSyntax>(2) { mapperClass };

#if ROSLYN4_7_OR_GREATER
        if (descriptor.UnsafeAccessors.Count > 0)
        {
            var unsafeAccessorClass = UnsafeAccessorEmitter.BuildUnsafeAccessorClass(descriptor, cancellationToken, ctx);
            compilationUnitMembers.Add(unsafeAccessorClass);
        }
#endif

        var compilationUnitMemberSyntaxList = List(compilationUnitMembers.SeparateByTrailingLineFeed(memberCtx.SyntaxFactory.Indentation));

        ctx = ctx.RemoveIndentation();
        compilationUnitMemberSyntaxList = WrapInClassesAsNeeded(ref ctx, descriptor.Symbol, compilationUnitMemberSyntaxList);
        compilationUnitMemberSyntaxList = WrapInNamespaceIfNeeded(ctx, descriptor.Namespace, compilationUnitMemberSyntaxList);

        return CompilationUnit()
            .WithMembers(compilationUnitMemberSyntaxList)
            .WithLeadingTrivia(Comment(AutoGeneratedComment), ElasticCarriageReturnLineFeed, Nullable(true), ElasticCarriageReturnLineFeed);
    }

    private static IEnumerable<MemberDeclarationSyntax> BuildMembers(
        SourceEmitterContext ctx,
        MapperDescriptor descriptor,
        CancellationToken cancellationToken
    )
    {
        foreach (var mapping in descriptor.MethodTypeMappings)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return mapping.BuildMethod(ctx);
        }
    }

    private static SyntaxList<MemberDeclarationSyntax> WrapInClassesAsNeeded(
        ref SourceEmitterContext ctx,
        INamedTypeSymbol symbol,
        SyntaxList<MemberDeclarationSyntax> members
    )
    {
        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            if (containingType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not ClassDeclarationSyntax containingTypeSyntax)
                break;

            members = SingletonList<MemberDeclarationSyntax>(
                ctx.SyntaxFactory.Class(containingType.Name, containingTypeSyntax.Modifiers, members)
            );
            ctx = ctx.RemoveIndentation();
            containingType = containingType.ContainingType;
        }

        return members;
    }

    private static SyntaxList<MemberDeclarationSyntax> WrapInNamespaceIfNeeded(
        SourceEmitterContext ctx,
        string? namespaceName,
        SyntaxList<MemberDeclarationSyntax> members
    )
    {
        if (namespaceName == null)
            return members;

        return SingletonList<MemberDeclarationSyntax>(ctx.SyntaxFactory.Namespace(namespaceName).WithMembers(members));
    }

    private static SourceEmitterContext IndentForMapper(SourceEmitterContext ctx, INamedTypeSymbol symbol)
    {
        while (symbol.ContainingType != null)
        {
            ctx = ctx.AddIndentation();
            symbol = symbol.ContainingType;
        }

        return symbol.ContainingNamespace.ContainingNamespace == null ? ctx : ctx.AddIndentation();
    }
}
