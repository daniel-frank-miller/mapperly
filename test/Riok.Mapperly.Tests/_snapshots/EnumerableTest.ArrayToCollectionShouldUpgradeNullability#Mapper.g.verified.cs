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
            target.Value = MapToICollection(source.Value);
        }
        else
        {
            target.Value = null;
        }
        return target;
    }

    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "0.0.1.0")]
    private global::System.Collections.Generic.ICollection<string?> MapToICollection(int[] source)
    {
        var target = new global::System.Collections.Generic.List<string?>(source.Length);
        foreach (var item in source)
        {
            target.Add(item.ToString());
        }
        return target;
    }
}