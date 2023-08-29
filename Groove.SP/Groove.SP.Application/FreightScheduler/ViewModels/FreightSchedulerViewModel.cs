using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.FreightScheduler.Validations;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Groove.SP.Application.FreightScheduler.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class FreightSchedulerViewModel : ViewModelBase<FreightSchedulerModel>, IHasFieldStatus
    {
        public long Id { set; get; }
        public string ModeOfTransport { set; get; }
        public string CarrierCode { set; get; }
        public string CarrierName { set; get; }

        public string VesselName { set; get; }
        public string Voyage { set; get; }
        public string MAWB { set; get; }
        public string FlightNumber { set; get; }

        public string LocationFromCode { get; set; }
        public string LocationFromName { set; get; }

        public string LocationToCode { get; set; }
        public string LocationToName { set; get; }

        public DateTime ETDDate { set; get; }
        public DateTime ETADate { set; get; }

        public DateTime? ATDDate { set; get; }
        public DateTime? ATADate { set; get; }

        public DateTime? CYOpenDate { set; get; }
        public DateTime? CYClosingDate { set; get; }

        public bool IsHasLinkedItineraries { set; get; }

        public bool IsAllowExternalUpdate { set; get; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new FreightSchedulerValidation(isUpdating).ValidateAndThrow(this);
        }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
