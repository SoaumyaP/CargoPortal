using System.Threading.Tasks;
using Groove.SP.Application.POFulfillment.ViewModels;

namespace Groove.SP.Application.POFulfillment.Services.Interfaces
{
    public interface IEdiSonConfirmService
    {
        Task ConfirmPOFFAsync(EdiSonConfirmPOFFViewModel importVM);
    }
}
