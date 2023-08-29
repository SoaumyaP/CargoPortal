using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Application.Scheduling.ViewModels
{
    /// <summary>
    /// Model for list of Telerik scheduled tasks
    /// </summary>
    public class SchedulingListViewModel : ViewModelBase<SchedulingModel>
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string ReportGroup { get; set; }

        public string ReportName { get; set; }

        public SchedulingStatus Status { get; set; }

        public DateTime? NextOccurrence { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
