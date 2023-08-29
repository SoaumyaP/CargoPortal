using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.Services
{
    public class SOFormGeneratorContext : ISOFormGeneratorContext
    {
        private ISOFormGeneratorStrategy _generatorStrategy;
        public void SetStrategy(ISOFormGeneratorStrategy generatorStrategy)
        {
            _generatorStrategy = generatorStrategy;
        }

        public Task<byte[]> ExecuteStrategyAsync(ShippingOrderFormViewModel shippingOrderForm)
        {
            return _generatorStrategy.GenerateAsync(shippingOrderForm);
        }
    }
}
