using Groove.CSFE.Supplemental.Models;

namespace Groove.CSFE.Supplemental.Services
{
    public interface IBalanceOfGoodsService
    {
        ValueTask<PagedResultModel<BalanceOfGoodModel>> GetBalanceOfGoodAsync(BalanceOfGoodSearchModel model);
        ValueTask<PagedResultModel<BalanceOfGoodModel>> GetBalanceOfGoodAsync(FilterRoot model);
        ValueTask<PagedResultModel<BalanceOfGoodsTransactionModel>> GetBalanceOfGoodDetailAsync(string mode, int principleId, int? articleId, int? warehouseId, FilterRoot model);
    }
}
