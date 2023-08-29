using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities;

public class EmailSettingModel : Entity
{
    public long Id { get; set; }
    public long BuyerComplianceId { get; set; }
    public EmailSettingType EmailType { get; set; }
    public bool DefaultSendTo { get; set; }
    public string SendTo { get; set; }
    public string CC { get; set; }
    public BuyerComplianceModel BuyerCompliance { get; set; }
}