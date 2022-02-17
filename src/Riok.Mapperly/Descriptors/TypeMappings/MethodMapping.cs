using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.TypeMappings;

/// <summary>
/// Represents a mapping which is not a single expression but an entire method.
/// </summary>
public abstract class MethodMapping : TypeMapping
{
    private const string SourceParamName = "source";
    private string? _methodName;

    protected MethodMapping(ITypeSymbol sourceType, ITypeSymbol targetType) : base(sourceType, targetType)
    {
    }

    protected Accessibility Accessibility { get; set; } = Accessibility.Private;

    protected bool Override { get; set; }

    protected string MethodName
    {
        get => _methodName ?? throw new InvalidOperationException();
        set => _methodName = value;
    }

    public override ExpressionSyntax Build(ExpressionSyntax source)
        => Invocation(MethodName, source);

    public MethodDeclarationSyntax BuildMethod()
    {
        TypeSyntax returnType = ReturnType == null
            ? PredefinedType(Token(SyntaxKind.VoidKeyword))
            : IdentifierName(TargetType.ToDisplayString());

        return MethodDeclaration(returnType, Identifier(MethodName))
            .WithModifiers(TokenList(BuildModifiers()))
            .WithParameterList(BuildParameterList())
            .WithBody(Block(BuildBody(IdentifierName(SourceParamName))));
    }

    public abstract IEnumerable<StatementSyntax> BuildBody(ExpressionSyntax source);

    internal void SetMethodNameIfNeeded(Func<MethodMapping, string> methodNameBuilder)
    {
        _methodName ??= methodNameBuilder(this);
    }

    protected virtual ITypeSymbol? ReturnType => TargetType;

    protected virtual IEnumerable<ParameterSyntax> BuildParameters()
    {
        return new[]
        {
            Parameter(Identifier(SourceParamName)).WithType(IdentifierName(SourceType.ToDisplayString())),
        };
    }

    private IEnumerable<SyntaxToken> BuildModifiers()
    {
        yield return Accessibility(Accessibility);

        if (Override)
            yield return Token(SyntaxKind.OverrideKeyword);
    }

    private ParameterListSyntax BuildParameterList()
    {
        return ParameterList(CommaSeparatedList(BuildParameters()));
    }
}
