// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppException.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//    Application exception
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Groove.CSFE.Application.Exceptions
{
    public class AppException : Exception
    {
        public object AdditionalData { get; set; }

        public AppException(Exception ex, string message, params object[] placeholders)
            : base(GetMessage(message, placeholders), ex)
        {
        }

        public AppException(string message, params object[] placeholders)
            : base(GetMessage(message, placeholders))
        {
        }

        public bool IsDbConcurrencyUpdate { get; set; }

        public static string GetTrueExceptionMessage(Exception ex)
        {
            var innerMostException = ex;

            while (innerMostException?.InnerException != null)
            {
                innerMostException = innerMostException.InnerException;
            }

            return innerMostException?.Message ?? string.Empty;
        }

        private static string GetMessage(string message, object[] placeholders)
        {
            if (placeholders == null || placeholders.Length == 0)
            {
                return message;
            }

            return string.Format(message, placeholders);
        }
    }
}