﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    public partial object Map(object source, global::System.Type destinationType, global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        return source switch
        {
            global::A x when destinationType.IsAssignableFrom(typeof(global::B)) => MapToB(x, refHandler),
            null => throw new System.ArgumentNullException(nameof(source)),
            _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {destinationType} as there is no known type mapping", nameof(source)),
        };
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::B MapToB(global::A source, global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<global::A, global::B>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new global::B();
        refHandler.SetReference<global::A, global::B>(source, target);
        return target;
    }
}