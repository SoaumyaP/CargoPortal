namespace Groove.CSFE.Core.Entities
{
    public class TerminalModel : Entity
    {
        public long Id { get; set; }
        public long LocationId { get; set; }
        public string TerminalCode { get; set; }
        public string TerminalName { get; set; }
        public string Address { get; set; }

        public virtual LocationModel Location { get; set; }
    }
}
