using System;
using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.Itinerary.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ItineraryViewModel : ViewModelBase<ItineraryModel>, IHasFieldStatus, IHasViewSetting
    {
        public long Id { get; set; }

        public int? Sequence { get; set; }

        public string ModeOfTransport { get; set; }

        public string CarrierName { get; set; }

        public string SCAC { get; set; }

        public string AirlineCode { get; set; }

        public string VesselFlight { get; set; }

        public string VesselName { get; set; }

        public string Voyage { get; set; }

        public string FlightNumber { get; set; }

        /// <summary>
        /// To contain location information of loading port. It contains 2 business data:
        /// </summary>
        /// <value>
        /// <list type="number">
        /// <item>
        /// From Itinerary Import/Update API, it contains location code
        /// </item>
        /// <item>
        /// From Front-end, it contains location description
        /// </item>
        /// </list>
        /// </value>
        public string LoadingPort { get; set; }

        public DateTime ETDDate { get; set; }

        public DateTime ETADate { get; set; }

        public DateTime? ATDDate { get; set; }

        public DateTime? ATADate { get; set; }

        /// <summary>
        /// To contain location information of discharge port. It contains 2 business data:
        /// </summary>
        /// <value>
        /// <list type="number">
        /// <item>
        /// From Itinerary Import/Update API, it contains location code
        /// </item>
        /// <item>
        /// From Front-end, it contains location description
        /// </item>
        /// </list>
        /// </value>
        public string DischargePort { get; set; }

        public string RoadFreightRef { get; set; }

        public string Status { get; set; }

        public bool IsImportFromApi { get; set; }

        /// <summary>
        /// It is true if called from application
        /// </summary>
        public bool IsCalledFromApp { get; set; }

        public long? ConsignmentId { get; set; }

        public long? ScheduleId { get; set; }

        public DateTime? CYOpenDate { set; get; }

        public DateTime? CYClosingDate { set; get; }

        public string ViewSettingModuleId { get; set; }

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { set; get; }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();

        }
    }
}
