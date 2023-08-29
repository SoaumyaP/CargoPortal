using System;

namespace Groove.SP.Application.Scheduling.ViewModels
{
    /// <summary>
    /// Model of activity on Telerik scheduled task
    /// </summary>
    public class TelerikActivityModel
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public DateTime DateCreated { get; set; }
        public string DocumentName { get; set; }
        public string Error { get; set; }
        public bool HasDocument { get; set; }
        public bool Succeed { get; set; }

    }
}
