using Groove.SP.Application.POFulfillment.ViewModels;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.Services.Interfaces
{
    public interface ISOFormGeneratorContext
    {
        void SetStrategy(ISOFormGeneratorStrategy strategy);

        Task<byte[]> ExecuteStrategyAsync(ShippingOrderFormViewModel shippingOrderForm);
    }
}