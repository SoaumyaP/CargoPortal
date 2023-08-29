using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Application.Shipments.Validations;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Groove.SP.Application.Shipments.ViewModels;

[JsonConverter(typeof(MyConverter))]
public class ImportShipmentViewModel : ViewModelBase<ShipmentModel>, IHasFieldStatus
{
    public ImportShipmentStatus? Status { get; init; }
    public string ShipmentNo { get; init; }
    public string BuyerCode { get; init; }
    public string CustomerReferenceNo { get; init; }
    public string AgentReferenceNo { get; init; }
    public string ShipperReferenceNo { get; init; }
    public string BookingReferenceNo { get; init; }
    public string CarrierContractNo { get; init; }
    public string ModeOfTransport { get; set; }
    public string ShipFrom { get; init; }
    public DateTime ShipFromETDDate { get; init; }
    public string ShipTo { get; init; }
    public DateTime ShipToETADate { get; init; }
    public string Movement { get; init; }
    public string ServiceType { get; set; }
    public string Incoterm { get; set; }
    public int? Factor { get; set; }
    public string HouseNo { get; init; }
    public string HouseBillType { get; init; }
    public string JobNumber { get; init; }
    public DateTime? HouseIssueDate { get; init; }
    public string MasterNo { get; init; }
    public bool? IsDirectMaster { get; init; }
    public string PlaceOfIssue { get; init; }
    public DateTime? MasterIssueDate { get; init; }
    public DateTime? OnBoardDate { get; init; }
    public ICollection<ImportShipmentContactViewModel> Contacts { get; init; }
    public ICollection<ImportShipmentContainerViewModel> Containers { get; init; }
    public ICollection<ImportShipmentCargoDetailViewModel> CargoDetails { get; init; }
    public ICollection<ImportItineraryViewModel> Itineraries { get; set; }

    public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
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

    public bool Validate(ICSFEApiClient csfeApiClient, IShipmentRepository shipmentRepository, IContractMasterRepository contractMasterRepository, out ImportingShipmentResultViewModel result)
    {
        result = new();
        if (!IsPropertyDirty(nameof(Status)) || !Status.HasValue)
        {
            result.LogErrors($"{nameof(Status)} must not be empty.");

            return false;
        }
        var validator = new ImportShipmentValidator(csfeApiClient, shipmentRepository, contractMasterRepository, Status.Value);
        var validationResult = validator.Validate(this);

        if (!validationResult.IsValid)
        {
            // By default, the fluent validation returns an error message like "'Organization Role' must not be empty."
            // but we must be return 'Contacts[0].OrganizationRole must not be empty.' which is combined from PropertyName and ErrorMessage
            // so we need to replace the text in single quotes ('') with PropertyName.
            string pattern = @"(^['-])([a-zA-Z\s]+)(['-])";
            Regex rg = new(pattern, RegexOptions.IgnoreCase);

            foreach (var item in validationResult.Errors)
            {
                var message = rg.Replace(item.ErrorMessage, item.PropertyName);
                result.LogErrors(message);
            }
        }
        return validationResult.IsValid;
    }
}

/// <summary>
/// To contain result of shipment importation. 
/// <b>It is designed to naming on camelcase object serialization</b>
/// <br></br>
/// <code>
/// {
///    "success": false,
///    "results": {
///        "errors": {
///            "messages": [
///                "ShipmentNo cannot be duplicated.",
///                "Contacts[3].CompanyName must not be empty."
///            ]
///        }
///    }
///}
/// </code>
/// </summary>
public class ImportingShipmentResultViewModel
{
    public bool Success { get; set; }
    public Dictionary<string, ExpandoObject> Results { get; set; }

    [JsonIgnore]
    private List<string> Messages { get; set; }

    public ImportingShipmentResultViewModel()
    {
        Success = true;
        Results = new();
        Messages = new();
    }

    public void LogSuccess(string logName, string logContext)
    {
        Success = true;
        if (!Results.ContainsKey("data"))
        {
            Results = new Dictionary<string, ExpandoObject>
                {
                    { "data", new ExpandoObject() }
                };
        }
        var value = Results["data"];
        value.TryAdd(StringHelper.FirstCharToLowerCase(logName), logContext);
    }

    public void LogErrors(string error)
    {
        Success = false;
        if (!Results.ContainsKey("errors"))
        {
            Results = new Dictionary<string, ExpandoObject>
            {
                { "errors", new ExpandoObject() }
            };
        }
        var value = Results["errors"];

        Messages.Add(error.Trim());

        value.TryAdd("messages", Messages);
    }
}

public enum ImportShipmentStatus
{
    /// <summary>
    /// New
    /// </summary>
    N,
    /// <summary>
    /// Update
    /// </summary>
    U,
    /// <summary>
    /// Delete
    /// </summary>
    D
};