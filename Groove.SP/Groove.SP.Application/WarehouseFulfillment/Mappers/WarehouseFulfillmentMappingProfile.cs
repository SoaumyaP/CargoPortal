using AutoMapper;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Linq;

namespace Groove.SP.Application.WarehouseFulfillment.Mappers
{
    public class WarehouseFulfillmentMappingProfile : MappingProfileBase<POFulfillmentModel, WarehouseFulfillmentViewModel>
    {
        public WarehouseFulfillmentMappingProfile()
        {
            CreateMap<POFulfillmentModel, WarehouseFulfillmentViewModel>().ReverseMap();
            CreateMap<WarehouseFulfillmentOrderViewModel, POFulfillmentOrderModel>().ReverseMap();
            CreateMap<WarehouseFulfillmentContactViewModel, POFulfillmentContactModel>()
                // To split address into address lines
                .ForMember(d => d.Address, opt => opt.MapFrom(y => SplitCompanyAddressLines(y.Address, 1)))
                .ForMember(d => d.AddressLine2, opt => opt.MapFrom(y => SplitCompanyAddressLines(y.Address, 2)))
                .ForMember(d => d.AddressLine3, opt => opt.MapFrom(y => SplitCompanyAddressLines(y.Address, 3)))
                .ForMember(d => d.AddressLine4, opt => opt.MapFrom(y => SplitCompanyAddressLines(y.Address, 4)))
                .ReverseMap()
                .ForMember(d => d.Address, opt => opt.MapFrom<ConcatenateCompanyAddressLinesResolver>());
            CreateMap<InputWarehouseFulfillmentViewModel, POFulfillmentModel>()
                .ForMember(x => x.ReceiptPort, opt => opt.MapFrom(o => string.Empty))
                .ForMember(x => x.DeliveryPort, opt => opt.MapFrom(o => string.Empty))
                .ForMember(x => x.ExpectedShipDate, opt => opt.MapFrom(o => string.Empty))
                .ForMember(x => x.AgentAssignmentMode, opt => opt.MapFrom(o => string.Empty));
            CreateMap<POFulfillmentOrderModel, WarehouseOrderSOFormViewModel>();
        }

        /// <summary>
        /// It is to split address information into some sections (Address, address line 2, address line 3, and address line 4) by newline \n
        /// </summary>
        /// <param name="concatenatedAddress">Concatenated address information</param>
        /// <param name="lineNumber">Line number</param>
        /// 1 -> Address
        /// 2 -> Address line 2
        /// 3 -> Address line 3
        /// 4 -> Address line 4
        /// <returns></returns>
        public static string SplitCompanyAddressLines(string concatenatedAddress, int lineNumber)
        {
            if (!string.IsNullOrEmpty(concatenatedAddress))
            {
                var addressLines = concatenatedAddress.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                if (addressLines != null && addressLines.Any() && lineNumber <= addressLines.Count())
                {
                    if (lineNumber == 4)
                    {
                        var tmp = addressLines[lineNumber - 1];
                        for (int i = lineNumber; i < addressLines.Count(); i++)
                        {
                            tmp += "\n" + addressLines[i];
                        }
                        return tmp;
                    }
                    return addressLines[lineNumber - 1];
                }
                return null;
            }
            return null;
        }
    }
    public interface IConcatenateCompanyAddressLinesResolver<in TSource, in TDestination, TDestMember> : IValueResolver<TSource, TDestination, TDestMember>
    {
    }
    public class ConcatenateCompanyAddressLinesResolver : IConcatenateCompanyAddressLinesResolver<POFulfillmentContactModel, WarehouseFulfillmentContactViewModel, string>
    {
        public string Resolve(POFulfillmentContactModel source, WarehouseFulfillmentContactViewModel destination, string member, ResolutionContext context)
        {
            return ConcatenateCompanyAddressLines(source.Address, source.AddressLine2, source.AddressLine3, source.AddressLine4);
        }

        /// <summary>
        /// It is to concatenate address lines to address information with newline \n
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="addressLine2">Address line 2</param>
        /// <param name="addressLine3">Address line 3</param>
        /// <param name="addressLine4">Address line 4</param>
        /// <returns></returns>
        public static string ConcatenateCompanyAddressLines(string address, string addressLine2, string addressLine3, string addressLine4)
        {
            var result = address;

            if (!string.IsNullOrEmpty(addressLine2))
            {
                result = (!string.IsNullOrEmpty(result) ? (result + "\n") : "") + addressLine2;
            }
            if (!string.IsNullOrEmpty(addressLine3))
            {
                result += "\n" + addressLine3;
            }
            if (!string.IsNullOrEmpty(addressLine4))
            {
                result += "\n" + addressLine4;
            }
            return result;
        }
    }

}
