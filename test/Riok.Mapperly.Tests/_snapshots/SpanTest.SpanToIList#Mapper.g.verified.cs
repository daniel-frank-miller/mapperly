﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::System.Collections.Generic.IList<int> Map(global::System.Span<int> source)
    {
        return MapToList(source);
    }

    private global::System.Collections.Generic.List<int> MapToList(global::System.Span<int> source)
    {
        var target = new global::System.Collections.Generic.List<int>();
        target.EnsureCapacity(source.Length + target.Count);
        foreach (var item in source)
        {
            target.Add(item);
        }
        return target;
    }
}