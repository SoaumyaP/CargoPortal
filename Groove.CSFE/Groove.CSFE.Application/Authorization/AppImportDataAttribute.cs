using System;

namespace Groove.CSFE.Application.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AppImportDataAttribute : Attribute
    {
        public AppImportDataAttribute()
        {
        }
    }
}
