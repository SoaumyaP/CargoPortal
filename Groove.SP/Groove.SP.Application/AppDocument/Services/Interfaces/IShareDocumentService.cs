using System.Threading.Tasks;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.AppDocument.Services.Interfaces
{
    public interface IShareDocumentService : IServiceBase<ShareDocumentModel, ShareDocumentViewModel>
    {
        Task<ShareDocumentViewModel> GetAsync(long id);
    }
}
