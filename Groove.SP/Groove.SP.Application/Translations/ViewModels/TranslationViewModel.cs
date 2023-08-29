using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Translations.ViewModels
{
    public class TranslationViewModel : ViewModelBase<TranslationModel>
    {
        public string Key { get; set; }

        public string English { get; set; }

        public string TraditionalChinese { get; set; }

        public string SimplifiedChinese { get; set; }

        public TranslationViewModel()
            : base()
        {
        }
        
        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
