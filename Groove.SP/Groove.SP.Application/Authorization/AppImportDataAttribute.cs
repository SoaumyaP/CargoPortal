using System;

namespace Groove.SP.Application.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AppImportDataAttribute : Attribute
    {
        public AppImportDataAttribute()
        {
        }
    }
}
