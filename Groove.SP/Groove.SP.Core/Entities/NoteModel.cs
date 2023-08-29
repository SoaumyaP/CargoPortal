
namespace Groove.SP.Core.Entities
{
    public class NoteModel : Entity
    {
        #region Properties

        public long Id { get; set; }

        public string GlobalObjectId { get; set; }

        public string Category { get; set; }

        public string NoteText { get; set; }

        public string ExtendedData { get; set; }

        //Name of user (in The Portal database) who created note
        public string Owner { get; set; }


        #endregion
    }
}
