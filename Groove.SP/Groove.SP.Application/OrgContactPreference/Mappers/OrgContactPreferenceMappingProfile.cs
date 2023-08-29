using AutoMapper;
using Groove.SP.Application.BulkFulfillment.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Application.OrgContactPreference.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.OrgContactPreference.Mappers
{
    public class OrgContactPreferenceMappingProfile : MappingProfileBase<OrgContactPreferenceModel, OrgContactPreferenceViewModel>
    {
        public OrgContactPreferenceMappingProfile()
        {
            CreateMap<BulkFulfillmentContactViewModel, OrgContactPreferenceViewModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<OrgContactPreferenceViewModel, OrgContactPreferenceModel>();

            CreateMap<OrgContactPreferenceModel, OrgContactPreferenceViewModel>()
                .ForMember(d => d.Address, opt => opt.MapFrom<ConcatenateCompanyAddressLinesResolver>());
        }
    }

    public interface IConcatenateCompanyAddressLinesResolver<in TSource, in TDestination, TDestMember> : IValueResolver<TSource, TDestination, TDestMember>
    {
    }

    public class ConcatenateCompanyAddressLinesResolver : IConcatenateCompanyAddressLinesResolver<OrgContactPreferenceModel, OrgContactPreferenceViewModel, string>
    {
        public string Resolve(OrgContactPreferenceModel source, OrgContactPreferenceViewModel destination, string member, ResolutionContext context)
        {
            return ConcatenateCompanyAddressLines(source.Address, source.AddressLine2, source.AddressLine3, source.AddressLine4);
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
