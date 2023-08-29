using AutoMapper;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Application.WarehouseLocations.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.WarehouseLocations.Mappers
{
    public class WarehouseLocationMappingProfile : MappingProfileBase<WarehouseLocationModel, WarehouseLocationViewModel>
    {
        public WarehouseLocationMappingProfile()
        {
            CreateMap<WarehouseLocationModel, WarehouseLocationViewModel>()
                .ForMember(d => d.Address, opt => opt.MapFrom<ConcatenateCompanyAddressLinesResolver>())
                .ReverseMap();
        }
    }

    public interface IConcatenateCompanyAddressLinesResolver<in TSource, in TDestination, TDestMember> : IValueResolver<TSource, TDestination, TDestMember>
    {
    }

    public class ConcatenateCompanyAddressLinesResolver : IConcatenateCompanyAddressLinesResolver<WarehouseLocationModel, WarehouseLocationViewModel, string>
    {
        public string Resolve(WarehouseLocationModel source, WarehouseLocationViewModel destination, string member, ResolutionContext context)
        {
            return ConcatenateCompanyAddressLines(source.AddressLine1, source.AddressLine2, source.AddressLine3, source.AddressLine4);
        }

        /// <summary>
        /// It is to concatenate address lines to address information with newline \n
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="addressLine2">Address line 2</param>
        /// <param name="addressLine3">Address line 3</param>
        /// <param name="addressLine4">Address line 4/param>
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
