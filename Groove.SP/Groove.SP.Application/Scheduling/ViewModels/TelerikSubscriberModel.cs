namespace Groove.SP.Application.Scheduling.ViewModels
{
    /// <summary>
    /// Model of subscribers on Telerik scheduled task
    /// </summary>
    public class TelerikSubscriberModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }

    }
}
