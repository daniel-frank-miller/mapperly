using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Emit.Syntax;
using Riok.Mapperly.Helpers;
using Riok.Mapperly.Symbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Riok.Mapperly.Emit.Syntax.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Mappings.UserMappings;

/// <summary>
/// A mapping which has a <see cref="Type"/> as a second parameter describing the target type of the mapping.
/// Generates a switch expression based on the mapping types.
/// </summary>
public abstract class UserDefinedNewInstanceRuntimeTargetTypeMapping(
    IMethodSymbol method,
    MethodParameter sourceParameter,
    MethodParameter? referenceHandlerParameter,
    ITypeSymbol targetType,
    bool enableReferenceHandling,
    NullFallbackValue nullArm,
    ITypeSymbol objectType
) : NewInstanceMethodMapping(method, sourceParameter, referenceHandlerParameter, targetType), IUserMapping
{
    private const string IsAssignableFromMethodName = nameof(Type.IsAssignableFrom);
    private const string GetTypeMethodName = nameof(GetType);

    private readonly List<RuntimeTargetTypeMapping> _mappings = new();

    // requires the user mapping bodies
    // as the delegate mapping of user mappings is only set after bodies are built
    // and if reference handling is enabled,
    // but the user mapping does not have a reference handling parameter,
    // only the delegate mapping is callable by other mappings.
    public override MappingBodyBuildingPriority BodyBuildingPriority => MappingBodyBuildingPriority.AfterUserMappings;

    public IMethodSymbol Method { get; } = method;

    /// <summary>
    /// Always false, since <see cref="CallableByOtherMappings"/> is false
    /// this can never be the default.
    /// </summary>
    public bool? Default => false;

    public override bool CallableByOtherMappings => false;

    public void AddMappings(IEnumerable<RuntimeTargetTypeMapping> mappings) => _mappings.AddRange(mappings);

    public override IEnumerable<StatementSyntax> BuildBody(TypeMappingBuildContext ctx)
    {
        // if reference handling is enabled and no reference handler parameter is declared
        // a new reference handler is instantiated and used.
        if (enableReferenceHandling && ReferenceHandlerParameter == null)
        {
            // var refHandler = new RefHandler();
            var referenceHandlerName = ctx.NameBuilder.New(DefaultReferenceHandlerParameterName);
            var createRefHandler = ctx.SyntaxFactory.CreateInstance<PreserveReferenceHandler>();
            yield return ctx.SyntaxFactory.DeclareLocalVariable(referenceHandlerName, createRefHandler);

            ctx = ctx.WithRefHandler(referenceHandlerName);
        }

        var targetTypeExpr = BuildTargetType();

        // _ => throw new ArgumentException(msg, nameof(ctx.Source)),
        var sourceType = Invocation(MemberAccess(ctx.Source, GetTypeMethodName));
        var fallbackArm = SwitchArm(
            DiscardPattern(),
            ThrowArgumentExpression(
                InterpolatedString($"Cannot map {sourceType} to {targetTypeExpr} as there is no known type mapping"),
                ctx.Source
            )
        );

        // source switch { A x when targetType.IsAssignableFrom(typeof(ADto)) => MapToADto(x), B x when targetType.IsAssignableFrom(typeof(BDto)) => MapToBDto(x) }
        var (typeArmContext, typeArmVariableName) = ctx.WithNewScopedSource();
        var arms = _mappings.Select(x => BuildSwitchArm(typeArmContext, typeArmVariableName, x, targetTypeExpr));

        // null => default / throw
        arms = arms.Append(SwitchArm(ConstantPattern(NullLiteral()), NullSubstitute(TargetType, ctx.Source, nullArm)));
        arms = arms.Append(fallbackArm);
        var switchExpression = ctx.SyntaxFactory.Switch(ctx.Source, arms);
        yield return ctx.SyntaxFactory.Return(switchExpression);
    }

    protected abstract ExpressionSyntax BuildTargetType();

    protected virtual ExpressionSyntax? BuildSwitchArmWhenClause(ExpressionSyntax targetType, RuntimeTargetTypeMapping mapping)
    {
        // targetType.IsAssignableFrom(typeof(ADto))
        return Invocation(
            MemberAccess(targetType, IsAssignableFromMethodName),
            TypeOfExpression(FullyQualifiedIdentifier(mapping.Mapping.TargetType.NonNullable()))
        );
    }

    internal override void EnableReferenceHandling(INamedTypeSymbol iReferenceHandlerType)
    {
        // the parameters of user defined methods should not be manipulated
    }

    private SwitchExpressionArmSyntax BuildSwitchArm(
        TypeMappingBuildContext typeArmContext,
        string typeArmVariableName,
        RuntimeTargetTypeMapping mapping,
        ExpressionSyntax targetType
    )
    {
        // A x when targetType.IsAssignableFrom(typeof(ADto)) => MapToADto(x),
        var declaration = DeclarationPattern(
            FullyQualifiedIdentifier(mapping.Mapping.SourceType.NonNullable()).AddTrailingSpace(),
            SingleVariableDesignation(Identifier(typeArmVariableName))
        );
        var whenCondition = BuildSwitchArmWhenClause(targetType, mapping);
        var arm = SwitchArm(declaration, BuildSwitchArmMapping(mapping, typeArmContext));
        return whenCondition == null ? arm : arm.WithWhenClause(SwitchWhen(whenCondition));
    }

    private ExpressionSyntax BuildSwitchArmMapping(RuntimeTargetTypeMapping mapping, TypeMappingBuildContext ctx)
    {
        var mappingExpression = mapping.Mapping.Build(ctx);
        if (mapping.IsAssignableToMethodTargetType)
            return mappingExpression;

        // (TTarget)(object)MapToTarget(source);
        return CastExpression(
            FullyQualifiedIdentifier(TargetType),
            CastExpression(FullyQualifiedIdentifier(objectType), mappingExpression)
        );
    }
}
