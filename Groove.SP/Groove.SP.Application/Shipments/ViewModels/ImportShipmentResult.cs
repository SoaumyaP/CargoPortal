using Newtonsoft.Json;
using System.Collections.Generic;

namespace Groove.SP.Application.Shipments.ViewModels;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class ImportShipmentResult
{
    public long? ShipmentId { get; set; }

    public string ShipmentNo { get; set; }

    public bool Success { get; set; }

    public List<System.ComponentModel.DataAnnotations.ValidationResult> Result { get; set; }
}