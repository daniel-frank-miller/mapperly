﻿#nullable enable
namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static partial class NestedTestMapper
    {
        public static partial class TestNesting
        {
            public static partial class NestedMapper
            {
                public static partial int ToInt(decimal value)
                {
                    return (int)value;
                }
            }
        }
    }
}