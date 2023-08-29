using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using System.Collections.Generic;

namespace Groove.SP.Application.CargoDetail.ViewModels;

public class ImportShipmentCargoDetailViewModel : CargoDetailViewModel
{
    public string PONumber { get; set; }
    public string ProductCode { get; set; }
    public string LineOrder { get; set; }
    public ICollection<ImportShipmentLoadDetailViewModel> LoadDetails { get; set; }
}