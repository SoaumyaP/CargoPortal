using Groove.CSFE.Supplemental.Models;
using Dapper;
using System.Reflection;
using Serilog;
using System.Data.SqlClient;
using System.Linq;

namespace Groove.CSFE.Supplemental.Services
{
    public class BalanceOfGoodsService : IBalanceOfGoodsService
    {
        private readonly IDbConnections connctions;
        private string[] supportedFields = new[] { "principlecode", "principlename", 
            "warehousecode", "warehousename", "locationname", 
            "articlecode", "articlename", "availablequantity", "receivedquantity", 
            "shippedquantity", "adjustquantity", "damagequantity"
        };
        private string[] numberFields = new[]
        {
            "availablequantity", "receivedquantity",
            "shippedquantity", "adjustquantity", "damagequantity",
            "noofpackage","packageuom","volumnecbm","grossweightkgs", "quantity"
        };
        private string[] supportedDetailFields = new[]
        {
            "principlecode", "principlename","articlecode","articlename",
            "warehousecode","warehousename","locationname","transtypecode", "transactiondate",
            "quantity","quantityuom","ponumber","sonumber","blnumber","documentnumber",
            "noofpackage","packageuom","volumnecbm","grossweightkgs"
        };
        private string[] dateField = new[] { "transactiondate" };

        public BalanceOfGoodsService(IDbConnections connctions)
        {
            this.connctions = connctions;
        }

        public async ValueTask<PagedResultModel<BalanceOfGoodModel>> GetBalanceOfGoodAsync(BalanceOfGoodSearchModel model)
        {
            string sql = !string.IsNullOrEmpty(model.Keyword) ?
                GetBalanceOfGoodByKeywordQuery(model.Keyword, model.AccessiblePrinciples) :
                GetBalanceOfGoodByFilterQuery(model.Principle, model.Article, model.Warehouse, model.Location, model.AccessiblePrinciples);
            return await GetPagedResultAsync<BalanceOfGoodModel>(supportedFields, sql, model.Order, model.Direction, model.Take, model.Skip);
            
        }

        async ValueTask<PagedResultModel<T>> GetPagedResultAsync<T>(IEnumerable<string> support, string sql, string sort, string dir, int take, int skip)
        {
            var countQuery = $"select count(*) as cnt from ({sql}) a";
            int count = await connctions.Supplemental.QuerySingleAsync<int>(countQuery);
            string order = !string.IsNullOrEmpty(sort) && support.Contains(sort.ToLower()) ?
                sort : support.First();
            string direction = !string.IsNullOrEmpty(dir) && dir.ToLower() == "asc" ? "asc" : "desc";
            sql += $" order by {order} {direction}";

            if (connctions.Supplemental is SqlConnection)
            {
                sql += $" OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
            }
            else
            {
                sql += $" limit {take} offset {skip}";
            }

            var data = await connctions.Supplemental.QueryAsync<T>(sql);
            var result = new PagedResultModel<T>(count, skip, take, order, direction, data);
            Log.Logger.Information(String.Format("Assembly: {0}, Action: {1}, Result: \"total\": {2}, \"record\": {3}",
                nameof(BalanceOfGoodsService), nameof(GetPagedResultAsync), count, data?.Count() ?? 0));
            return result;
        }

        private string GetBalanceOfGoodByKeywordQuery(
            string Keyword,
            IEnumerable<long>? principles)
        {
            string sql = rootQuery;
            if (principles != null && principles.Any())
            {
                sql += string.Format(" and PrincipleId in ({0})", string.Join(", ", principles));
            }
            if (!string.IsNullOrEmpty(Keyword))
            {
                string k = Keyword.Replace("'", "''");
                sql += $" and (PrincipleCode like '%{k}%' or PrincipleName like '%{k}%' or WarehouseCode like '%{k}%' or WarehouseName like '%{k}%' or LocationName like '%{k}%' or ArticleCode like '%{k}%' or ArticleName like '%{k}%')";
            }
            return sql;
        }

        private string GetBalanceOfGoodByFilterQuery(
            string? principle,
            string? article,
            string? warehouse,
            string? location,
            IEnumerable<long>? principles)
        {
            string sql = rootQuery;
            if (principles != null && principles.Any())
            {
                sql += string.Format(" and PrincipleId in ({0})", string.Join(", ", principles));
            }
            if (!string.IsNullOrEmpty(principle))
                sql += $" and (PrincipleCode like '%{principle.Replace("'", "''")}%' or PrincipleName like '%{principle.Replace("'", "''")}%')";

            if (!string.IsNullOrEmpty(article))
                sql += $" and ([ArticleCode] like '%{article.Replace("'", "''")}%' or [ArticleName] like '%{article.Replace("'", "''")}%')";

            if (!string.IsNullOrEmpty(warehouse))
                sql += $" and ([WarehouseCode] like '%{warehouse.Replace("'", "''")}%' or [WarehouseName] like '%{warehouse.Replace("'", "''")}%')";

            if (!string.IsNullOrEmpty(location))
                sql += $" and ([LocationName] like '%{location.Replace("'", "''")}%')";

            return sql;
        }

        public async ValueTask<PagedResultModel<BalanceOfGoodModel>> GetBalanceOfGoodAsync(FilterRoot model)
        {
            string sql = GetBalanceOfGoodByFilterQuery(model);
            (string sort, string dir) = model.sort.Any() ?
                (model.sort.First().field, model.sort.First().dir) :
                (supportedFields.First(), "asc");

            return await GetPagedResultAsync<BalanceOfGoodModel>(supportedFields, sql, sort, dir, model.take, model.skip);
        }

        string GetBalanceOfGoodByFilterQuery(FilterRoot model)
        {
            string sql = rootQuery;

            if (!model.isInternal && (model.principles == null || model.principles.Count() == 0)) throw new ArgumentNullException("Principle(s) is required");

            if (!model.isInternal)
            {
                sql += string.Format(" and PrincipleId in ({0})", string.Join(", ", model.principles));
            }

            return GetFilterQuery(supportedFields, sql, model);
        }

        string GetFilterQuery(IEnumerable<string> supported, string sql, FilterRoot model)
        {
            foreach (var f in model.filter.filters)
            {
                if (supported.Contains(f.field.ToLower()))
                {
                    string whereClause = f.@operator switch
                    {
                        "eq" => numberFields.Contains(f.field.ToLower()) ?
                            $" and ({f.field} = {f.value})" :
                            dateField.Contains(f.field.ToLower()) ?
                            $" and (convert(date, {f.field}, 23) = '{DateTime.Parse(f.value.ToString()).ToString("yyyy-MM-dd")}')" :
                            $" and ({f.field} = '{f.value.ToString().Replace("'", "''")}')",

                        "neq" => numberFields.Contains(f.field.ToLower()) ?
                            $" and ({f.field} <> {f.value})" :
                            dateField.Contains(f.field.ToLower()) ?
                            $" and (convert(date, {f.field}, 23) <> '{DateTime.Parse(f.value.ToString()).ToString("yyyy-MM-dd")}')" :
                            $" and ({f.field} <> '{f.value.ToString().Replace("'", "''")}')",

                        "contains" => numberFields.Contains(f.field.ToLower()) ?
                            throw new NotSupportedException($"{f.field} not support contain filter") :
                            $" and ({f.field} like '%{f.value.ToString().Replace("'", "''")}%')",

                        "doesnotcontains" => numberFields.Contains(f.field.ToLower()) ?
                            throw new NotSupportedException($"{f.field} not support does not contain filter") :
                            $" and ({f.field} not like '%{f.value.ToString().Replace("'", "''")}%')",

                        "startswith" => numberFields.Contains(f.field.ToLower()) ?
                            throw new NotSupportedException($"{f.field} not support start with filter") :
                            $" and ({f.field} like '{f.value.ToString().Replace("'", "''")}%')",

                        "endswith" => numberFields.Contains(f.field.ToLower()) ?
                            throw new NotSupportedException($"{f.field} not support end with filter") :
                            $" and ({f.field} like '%{f.value.ToString().Replace("'", "''")}')",

                        "isnull" => $" and ({f.field} is null)",

                        "isnotnull" => $" and ({f.field} is not null)",

                        "isempty" => numberFields.Contains(f.field.ToLower()) ?
                            throw new NotSupportedException($"{f.field} not support is emplty filter") :
                            $" and (isnull({f.field}, '') = '' )",

                        "isnotempty" => numberFields.Contains(f.field.ToLower()) ?
                            throw new NotSupportedException($"{f.field} not support is not emplty filter") :
                            $" and (isnull({f.field}, '') > '' )",

                        "gte" => numberFields.Contains(f.field.ToLower()) ?
                            $" and ({f.field} >= {f.value})" :
                            dateField.Contains(f.field.ToLower()) ?
                            $" and (convert(date, {f.field}, 23) >= '{DateTime.Parse(f.value.ToString()).ToString("yyyy-MM-dd")}')" :
                            throw new NotSupportedException($"{f.field} not support greater than or equal filter"),

                        "gt" => numberFields.Contains(f.field.ToLower()) ?
                            $" and ({f.field} > {f.value})" :
                            dateField.Contains(f.field.ToLower()) ?
                            $" and (convert(date, {f.field}, 23) > '{DateTime.Parse(f.value.ToString()).ToString("yyyy-MM-dd")}')" :
                            throw new NotSupportedException($"{f.field} not support greater than filter"),

                        "lte" => numberFields.Contains(f.field.ToLower()) ?
                            $" and ({f.field} <= {f.value})" :
                            dateField.Contains(f.field.ToLower()) ?
                            $" and (convert(date, {f.field}, 23) <= '{DateTime.Parse(f.value.ToString()).ToString("yyyy-MM-dd")}')" :
                            throw new NotSupportedException($"{f.field} not support less than or equal filter"),

                        "lt" => numberFields.Contains(f.field.ToLower()) ?
                            $" and ({f.field} < {f.value})" :
                            dateField.Contains(f.field.ToLower()) ?
                            $" and (convert(date, {f.field}, 23) < '{DateTime.Parse(f.value.ToString()).ToString("yyyy-MM-dd")}')" :
                            throw new NotSupportedException($"{f.field} not support less than filter"),

                        _ => ""
                    };
                    sql += whereClause;
                }
            }
            return sql;
        }

        public async ValueTask<PagedResultModel<BalanceOfGoodsTransactionModel>> GetBalanceOfGoodDetailAsync(string mode, int principleId, int? articleId, int? warehouseId, FilterRoot model)
        {
            string sql = GetBalanceOfGoodDetailQuery(mode, principleId, articleId, warehouseId, model);
            (string sort, string dir) = model.sort.Any() ?
                (model.sort.First().field, model.sort.First().dir) :
                (supportedDetailFields.First(), "asc");
            return await GetPagedResultAsync<BalanceOfGoodsTransactionModel>(supportedDetailFields, sql, sort, dir, model.take, model.skip);
        }

        string GetBalanceOfGoodDetailQuery(string mode, int principleId, int? articleId, int? warehouseId, FilterRoot model)
        {
            string sql = detailQuery + $" where principleId = { principleId}";
            if (mode.ToLower() == "article")
            {
                sql += $" and ArticleId = {articleId.Value}";
            }
            else if (mode.ToLower() == "warehouse")
            {
                sql += $" and Warehouseid = {warehouseId.Value}";
            }

            return GetFilterQuery(supportedDetailFields, sql, model);
        }

        private const string rootQuery = @"
SELECT [PrincipleId]
      ,[PrincipleCode]
      ,[PrincipleName]
      ,[WarehouseId]
      ,[WarehouseCode]
      ,[WarehouseName]
      ,[LocationId]
      ,[LocationName]
      ,[ArticleId]
      ,[ArticleCode]
      ,[ArticleName]
      ,[AvailableQuantity]
      ,[ReceivedQuantity]
      ,[ShippedQuantity]
      ,[AdjustQuantity]
      ,[DamageQuantity]
  FROM [vw_BalanceOfGoods_Transactions] 
WHERE 1 = 1";

        private const string detailQuery = @"
SELECT [Id]
      ,[PrincipleId]
      ,[PrincipleCode]
      ,[PrincipleName]
      ,[ArticleId]
      ,[ArticleCode]
      ,[ArticleName]
      ,[WarehouseId]
      ,[WarehouseCode]
      ,[WarehouseName]
      ,[LocationName]
      ,[TransTypeId]
      ,[TransTypeCode]
      ,[TransactionDate]
      ,[Quantity]
      ,[QuantityUOM]
      ,[PoNumber]
      ,[SoNumber]
      ,[BlNumber]
      ,[DocumentNumber]
      ,[NoOfPackage]
      ,[PackageUOM]
      ,[VolumneCBM]
      ,[GrossWeightKgs]
      ,[Remarks]
  FROM [vw_BalanceOfGoods_Details]";
    }
}
