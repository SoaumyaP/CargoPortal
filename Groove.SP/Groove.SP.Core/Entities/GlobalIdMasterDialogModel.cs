namespace Groove.SP.Core.Entities
{
    public class GlobalIdMasterDialogModel : Entity
    {
        public string GlobalId { get; set; }

        public long MasterDialogId { get; set; }

        public string ExtendedData { get; set; }

        public GlobalIdModel ReferenceEntity { get; set; }

        public MasterDialogModel MasterDialog { get; set; }
    }
}
