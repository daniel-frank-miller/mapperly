using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Riok.Mapperly.Descriptors.ObjectFactories;

public class ObjectFactoryCollection
{
    private readonly IReadOnlyCollection<ObjectFactory> _objectFactories;
    private readonly Dictionary<ITypeSymbol, ObjectFactory> _concreteObjectFactories = new(SymbolEqualityComparer.IncludeNullability);

    public ObjectFactoryCollection(IReadOnlyCollection<ObjectFactory> objectFactories)
    {
        _objectFactories = objectFactories;
    }

    public bool TryFindObjectFactory(ITypeSymbol sourceType, ITypeSymbol targetType, [NotNullWhen(true)] out ObjectFactory? objectFactory)
    {
        if (_concreteObjectFactories.TryGetValue(targetType, out objectFactory))
            return true;

        objectFactory = _objectFactories.FirstOrDefault(f => f.CanCreateType(sourceType, targetType));
        if (objectFactory == null)
            return false;

        _concreteObjectFactories[targetType] = objectFactory;
        return true;
    }
}
