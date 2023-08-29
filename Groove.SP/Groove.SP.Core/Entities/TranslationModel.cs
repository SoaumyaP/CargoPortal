namespace Groove.SP.Core.Entities
{
    public class TranslationModel : Entity
    {
        public string Key { get; set; }

        public string English { get; set; }

        public string TraditionalChinese { get; set; }

        public string SimplifiedChinese { get; set; }

        public string Note { get; set; }
    }
}
