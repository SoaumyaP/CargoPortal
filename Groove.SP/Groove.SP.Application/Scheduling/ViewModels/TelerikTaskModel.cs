using System;
using System.Collections.Generic;

namespace Groove.SP.Application.Scheduling.ViewModels
{
	/// <summary>
	/// Model for GUI Telerik scheduled task
	/// </summary>
	public class TelerikTaskModel
    {
        public string Id { get; set; }
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
        public DateTime? NextOccurence { get; set; }
        public string Parameters { get; set; }
        public string RecurrenceRule { get; set; }
        public string Report { get; set; }
        public string ReportId { get; set; }
        public DateTime StartDate { get; set; }
    }

	/// <summary>
	/// Result model of GUI Telerik scheduled task
	/// </summary>
	public class TelerikSchedulingResultModel
    {
        public IEnumerable<TelerikTaskModel> Data { get; set; }
        public long Total { get; set; }
        public object AggregateResults { get; set; }
        public object Errors { get; set; }
    }

	/// <summary>
	/// Model for API Telerik scheduled task
	/// </summary>
	public class TelerikAPIScheduledTaskModel
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public bool Enabled { get; set; }

		public string ReportId { get; set; }

		public string DocumentFormat { get; set; }

		public DateTime StartDate { get; set; }

		public string RecurrenceRule { get; set; }

		public Dictionary<string, object> Parameters { get; set; }

		public string MailSubject { get; set; }

		public string MailBody { get; set; }

		public List<string> ExternalEmails { get; set; }

		public List<string> SubscriberIds { get; set; }

		public string UserId { get; set; }
	}

}
