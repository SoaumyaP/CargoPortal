using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.Exceptions
{
    public class AppAuthorizationException : AppException
    {
        public AppAuthorizationException(Exception ex, string message, params object[] placeholders)
            : base(ex, message, placeholders)
        {
        }

        public AppAuthorizationException(string message, params object[] placeholders)
            : base(message, placeholders)
        {
        }
    }
}
