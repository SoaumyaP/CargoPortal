using Groove.SP.Application.Common;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Application.EmailSetting.ViewModels;

public class EmailSettingViewModel : ViewModelBase<EmailSettingModel>
{
    public long Id { get; set; }
    public EmailSettingType EmailType { get; set; }
    public string EmailTypeName => EnumHelper<EmailSettingType>.GetDisplayName(EmailType);
    public bool DefaultSendTo { get; set; }
    public string SendTo { get; set; }
    public string CC { get; set; }

    public override void ValidateAndThrow(bool isUpdating = false)
    {
        throw new NotImplementedException();
    }
}