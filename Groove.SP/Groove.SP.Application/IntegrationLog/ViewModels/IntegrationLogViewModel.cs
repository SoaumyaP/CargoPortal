using System;
using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.IntegrationLog.Validations;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.IntegrationLog.ViewModels
{
    public class IntegrationLogViewModel : ViewModelBase<IntegrationLogModel>
    {
        public long Id { get; set; }
        public string Profile { get; set; }
        public string APIName { get; set; }
        public string APIMessage { get; set; }
        public string EDIMessageType { get; set; }
        public string EDIMessageRef { get; set; }
        public DateTime PostingDate { get; set; }
        public IntegrationStatus Status { get; set; }
        public string Remark { get; set; }
        public string Response { get; set; }
        public string StatusName => EnumHelper<IntegrationStatus>.GetDisplayName(this.Status);
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new IntegrationLogValidation().ValidateAndThrow(this);
        }
    }
}
