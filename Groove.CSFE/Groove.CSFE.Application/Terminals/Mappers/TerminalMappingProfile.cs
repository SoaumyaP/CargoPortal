using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.Terminals.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Terminals.Mappers
{
    public class TerminalMappingProfile : MappingProfileBase<TerminalModel, TerminalViewModel>
    {
        public TerminalMappingProfile()
        {
            CreateMap<TerminalModel, TerminalViewModel>().ReverseMap();
        }
    }
}
