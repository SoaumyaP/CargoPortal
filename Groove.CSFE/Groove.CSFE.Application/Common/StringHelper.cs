using System.Collections.Generic;
using System.Linq;

namespace Groove.CSFE.Application.Common
{
    public static class StringHelper
    {
        public static IList<string> Split(string value, char sepatator)
        {
            return value?.Split(sepatator)?.ToList();
        }
    }
}