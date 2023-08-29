using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Application.Ports.ViewModels
{
    public class PortViewModel : ViewModelBase<PortModel>
    {
        public long Id { get; set; }
        public string AirportCode { get; set; }
        public string AlternativeName { get; set; }
        public string ChineseName { get; set; }
        public long CountryId { get; set; }
        public string CountryName { get; set; }
        public bool IsAirport { get; set; }
        public string Name { get; set; }
        public string SeaportCode { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}
