using Groove.SP.Application.POFulfillment.ViewModels;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.Services.Interfaces
{
    public interface ISOFormGeneratorStrategy
    {
        Task<byte[]> GenerateAsync(ShippingOrderFormViewModel shippingOrderForm);
    }
}