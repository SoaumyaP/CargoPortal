using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Application.Exceptions
{
    public class AppUserFriendlyException : AppException
    {
        public AppUserFriendlyException(Exception ex, string message, params object[] placeholders)
            : base(ex, message, placeholders)
        {
        }

        public AppUserFriendlyException(string message, params object[] placeholders)
            : base(message, placeholders)
        {
        }
    }
}
