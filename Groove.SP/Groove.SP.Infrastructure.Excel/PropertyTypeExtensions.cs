using System;

namespace Groove.SP.Infrastructure.Excel
{
    public static class PropertyTypeExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the current Type represents a nullable enumeration.
        /// </summary>
        /// <param name="t"></param>
        /// <returns>true if the current Type represents a nullable enumeration; otherwise, false.</returns>
        public static bool IsNullableEnum(this Type t)
        {
            Type u = Nullable.GetUnderlyingType(t);
            return (u != null) && u.IsEnum;
        }
    }
}
