// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropDownModel.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Defines the NameValueModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GGroove.CSFE.Application
{
    public class DropDownModel<TValue>
    {
        public TValue Value { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }
    }

    public class DropDownModel : DropDownModel<long>
    {
    }
}
