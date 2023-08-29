using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;
using Groove.SP.Application.AppDocument.Services.Interfaces;

namespace Groove.SP.Application.AppDocument.Services
{
    public class ShareDocumentService : ServiceBase<ShareDocumentModel, ShareDocumentViewModel>, IShareDocumentService
    {
        public ShareDocumentService(IUnitOfWorkProvider uow) : base(uow)
        {
        }

        public async Task<ShareDocumentViewModel> GetAsync(long id)
        {
            var model = await this.Repository.GetAsync(x => x.Id == id);
            var viewModel = Mapper.Map<ShareDocumentViewModel>(model);
            return viewModel;
        }
    }
}
