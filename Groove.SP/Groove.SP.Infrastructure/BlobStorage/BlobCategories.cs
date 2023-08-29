// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlobCategories.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Infrastructure.BlobStorage
{
    public static class BlobCategories
    {
        public const string SHARE = "SHARE"; // Document being shared will be temporary store in this category
        public const string ATTACHMENT = "ATTACHMENT"; // Document being shared will be temporary store in this category
        public const string INVOICE = "INVOICE"; // Document will be stored in this category
        public const string CSEDSHIPPINGDOC = "CSEDSHIPPINGDOC"; // Document will not be stored, just link to CSED via API
    }
}