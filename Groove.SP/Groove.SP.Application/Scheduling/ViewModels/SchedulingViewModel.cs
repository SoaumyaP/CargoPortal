using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Scheduling.Validations;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Scheduling.ViewModels
{
    /// <summary>
    /// Model of Telerik scheduled task
    /// </summary>
    public class SchedulingViewModel: ViewModelBase<SchedulingModel>
    {
        public long Id { get; set; }
        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public string Category { get; set; }
        public string CategoryId { get; set; }
        public string DocumentFormat { get; set; }
        public string DocumentFormatDescr { get; set; }
        public bool Enabled { get; set; }
        public bool IsRecurrent { get; set; }
        public string MailTemplateBody { get; set; }
        public string MailTemplateSubject { get; set; }
        public string Name { get; set; }
        public DateTime? NextOccurrence { get; set; }
        public string Parameters { get; set; }
        public string RecurrenceRule { get; set; }
        public string Report { get; set; }
        public string ReportId { get; set; }
        public DateTime StartDate { get; set; }
        public long CreatedOrganizationId { get; set; }
        public long CSPortalReportId { get; set; }
        public ReportQueryModel CSPortalReport { get; set; }
        public SchedulingStatus Status { get; set; }

        public string TelerikSchedulingId { get; set; }

        public IEnumerable<TelerikSubscriberModel> Subscribers { get; set; }
        public IEnumerable<TelerikActivityModel> Activities { get; set; }



        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new SchedulingViewModelValidator().ValidateAndThrow(this);
        }
    }
}
