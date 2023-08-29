using AutoMapper;
using Groove.SP.Application.BulkFulfillment.ViewModels;
using Groove.SP.Application.Mappers;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Groove.SP.Application.BulkFulfillment.Mappers
{
    public class BulkFulfillmentMappingProfile : MappingProfileBase<POFulfillmentModel, BulkFulfillmentViewModel>
    {
        public BulkFulfillmentMappingProfile()
        {
            CreateMap<POFulfillmentModel, BulkFulfillmentViewModel>().ReverseMap();

            CreateMap<InputBulkFulfillmentViewModel, POFulfillmentModel>();

            CreateMap<BulkFulfillmentContactViewModel, POFulfillmentContactModel>();
            CreateMap<BulkFulfillmentLoadViewModel, POFulfillmentLoadModel>().ReverseMap();
            CreateMap<BulkFulfillmentOrderViewModel, POFulfillmentOrderModel>().ReverseMap();

            CreateMap<POFulfillmentContactModel, BulkFulfillmentContactViewModel>()
                .ForMember(d => d.Address, opt => opt.MapFrom<ConcatenateCompanyAddressLinesResolver>());
        }
    }

    public interface IConcatenateCompanyAddressLinesResolver<in TSource, in TDestination, TDestMember> : IValueResolver<TSource, TDestination, TDestMember>
    {
    }

    public class ConcatenateCompanyAddressLinesResolver : IConcatenateCompanyAddressLinesResolver<POFulfillmentContactModel, BulkFulfillmentContactViewModel, string>
    {
        public string Resolve(POFulfillmentContactModel source, BulkFulfillmentContactViewModel destination, string member, ResolutionContext context)
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

    public interface ISplitCompanyAddressLinesResolver<in TSource, in TDestination, TDestMember> : IValueResolver<TSource, TDestination, TDestMember>
    {
    }

    public class SplitCompanyAddressLinesResolver : ISplitCompanyAddressLinesResolver<BulkFulfillmentContactViewModel, POFulfillmentContactModel, string>
    {
        private int _lineNumber;
        public SplitCompanyAddressLinesResolver(int lineNumber)
        {
            _lineNumber = lineNumber;
        }
        public string Resolve(BulkFulfillmentContactViewModel source, POFulfillmentContactModel destination, string member, ResolutionContext context)
        {
            return SplitCompanyAddressLines(source.Address, _lineNumber);
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
}
