// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppException.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//    Application exception
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Groove.SP.Application.Exceptions
{
    public class EdisonBookingException : AppException
    {
        /// <summary>
        /// Result from ediSON API
        /// </summary>
        public string EdisonResult { get; set; }

        public EdisonBookingException(Exception ex, string message, object additionalData, string edisonResult = "")
            : base(ex, message)
        {
            this.AdditionalData = additionalData;
            EdisonResult = edisonResult; 
        }

        public EdisonBookingException(string message, object additionalData, string edisonResult = "")
            : base(message )
        {
            this.AdditionalData = additionalData;
            EdisonResult = edisonResult;

        }
    }
}