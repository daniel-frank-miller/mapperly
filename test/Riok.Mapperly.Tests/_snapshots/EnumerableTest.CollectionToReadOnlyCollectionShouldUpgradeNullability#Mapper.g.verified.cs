﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private partial global::B? Map(global::A? source)
    {
        if (source == null)
            return default;
        var target = new global::B();
        if (source.Value != null)
        {
            target.Value = MapToIReadOnlyCollection(source.Value);
        }
        else
        {
            target.Value = null;
        }
        return target;
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private global::System.Collections.Generic.IReadOnlyCollection<string?> MapToIReadOnlyCollection(global::System.Collections.Generic.ICollection<int> source)
    {
        var target = new string?[source.Count];
        var i = 0;
        foreach (var item in source)
        {
            target[i] = item.ToString();
            i++;
        }
        return target;
    }
}