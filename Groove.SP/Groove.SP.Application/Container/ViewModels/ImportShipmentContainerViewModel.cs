using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Container.ViewModels;

[JsonConverter(typeof(MyConverter))]
public class ImportShipmentContainerViewModel : ViewModelBase<ContainerModel>, IHasFieldStatus
{
    public string ContainerNo { get; set; }

    public EquipmentType ContainerType { get; set; }

    public DateTime? LoadingDate { get; set; }

    public string SealNo { get; set; }

    public string SealNo2 { get; set; }

    /// <summary>
    /// Mapping with Containers.CarrierSONo
    /// </summary>
    public string CarrierBookingNo { get; set; }

    public DateTime? CFSCutoffDate { get; set; }

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
}

