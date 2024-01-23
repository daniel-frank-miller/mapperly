﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial object Map(object source, global::System.Type targetType)
    {
        var refHandler = new global::Riok.Mapperly.Abstractions.ReferenceHandling.PreserveReferenceHandler();
        return source switch
        {
            global::A x when targetType.IsAssignableFrom(typeof(global::B)) => MapToB1(x, refHandler),
            global::C x when targetType.IsAssignableFrom(typeof(global::D)) => MapToD1(x, refHandler),
            null => throw new System.ArgumentNullException(nameof(source)),
            _ => throw new System.ArgumentException($"Cannot map {source.GetType()} to {targetType} as there is no known type mapping", nameof(source)),
        };
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::B MapToB(global::A source)
    {
        return MapToB1(source, new global::Riok.Mapperly.Abstractions.ReferenceHandling.PreserveReferenceHandler());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::D MapToD(global::C source)
    {
        return MapToD1(source, new global::Riok.Mapperly.Abstractions.ReferenceHandling.PreserveReferenceHandler());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private global::B MapToB1(global::A source, global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<global::A, global::B>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new global::B();
        refHandler.SetReference<global::A, global::B>(source, target);
        return target;
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private global::D MapToD1(global::C source, global::Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
    {
        if (refHandler.TryGetReference<global::C, global::D>(source, out var existingTargetReference))
            return existingTargetReference;
        var target = new global::D();
        refHandler.SetReference<global::C, global::D>(source, target);
        return target;
    }
}