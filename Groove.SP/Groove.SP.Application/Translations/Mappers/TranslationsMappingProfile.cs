using Groove.SP.Application.Mappers;
using Groove.SP.Application.Translations.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Translations.Mappers
{
    public class TranslationsMappingProfile : MappingProfileBase<TranslationModel, TranslationViewModel>
    {
        public TranslationsMappingProfile()
        {
            //add more config here
            CreateMap<TranslationModel, TranslationViewModel>();
        }
    }
}
