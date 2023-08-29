using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Invoices.Mappers
{
    public class InvoiceMappingProfile : MappingProfileBase<InvoiceModel, InvoiceViewModel>
    {
        public InvoiceMappingProfile()
        {
            CreateMap<InvoiceModel, InvoiceViewModel>()
                .ForMember(opts => opts.InvoiceDate, dest => dest.MapFrom(src => src.InvoiceDate.Date))
                .ForMember(opts => opts.ETDDate, dest => dest.MapFrom(src => src.ETDDate))
                .ForMember(opts => opts.ETADate, dest => dest.MapFrom(src => src.ETADate));
            CreateMap<InvoiceViewModel, InvoiceModel>();
            //add more config here
            CreateMap<InvoiceUpdatePaymentViewModel, InvoiceModel>()
                .ForAllMembers(des => des.Condition(src => src.IsPropertyDirty(des.DestinationMember.Name) && !des.DestinationMember.Name.Equals(nameof(InvoiceModel.InvoiceNo), System.StringComparison.OrdinalIgnoreCase)));
            CreateMap<InvoiceModel, InvoiceUpdatePaymentViewModel>();
        }
    }
}
