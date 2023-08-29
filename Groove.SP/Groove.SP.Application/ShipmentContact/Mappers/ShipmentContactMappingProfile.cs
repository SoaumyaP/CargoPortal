using Groove.SP.Application.Mappers;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Linq;

namespace Groove.SP.Application.ShipmentContact.Mappers
{
    public class ShipmentContactMappingProfile : MappingProfileBase<ShipmentContactModel, ShipmentContactViewModel>
    {
        public ShipmentContactMappingProfile()
        {
            CreateMap<ShipmentContactModel, ShipmentContactViewModel>()
                .ReverseMap();

            CreateMap<ShipmentContactViewModel, ShipmentContactModel>()
                .ForAllMembers(x => x.Condition(src => src.IsPropertyDirty(x.DestinationMember.Name)));

            CreateMap<ImportShipmentContactViewModel, ShipmentContactModel>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => CompanyAddressLinesResolver.ConcatenateCompanyAddressLines(src.AddressLine1, src.AddressLine2, src.AddressLine3, src.AddressLine4)));
        }
    }

    public class CompanyAddressLinesResolver
    {
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
        public static string SplitCompanyAddressLines(string concatenatedAddress, int lineNumber, string splitBy = "\n")
        {
            if (!string.IsNullOrEmpty(concatenatedAddress))
            {
                var addressLines = concatenatedAddress.Split(splitBy, StringSplitOptions.RemoveEmptyEntries);
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
}
