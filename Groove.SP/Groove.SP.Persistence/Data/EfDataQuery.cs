using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Core.Data;
using Groove.SP.Persistence.Contexts;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Groove.SP.Core.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Groove.SP.Persistence.Data
{
    public class EfDataQuery : IDataQuery
    {
        private readonly DbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppDbConnections _dataConnections;

        public EfDataQuery(IServiceProvider serviceProvider, AppDbConnections dataConnections)
        {
            _serviceProvider = serviceProvider;
            _dbContext = (DbContext)_serviceProvider.GetService(typeof(SpContext));
            // Inject application data connections: Csportaldb
            _dataConnections = dataConnections;
        }

        public IQueryable<TQuery> GetQueryable<TQuery>(string sql, params object[] parameters) where TQuery : class
        {
            return _dbContext.Set<TQuery>().FromSqlRaw(sql, parameters);
        }

        public TDataResult GetDataBySql<TDataResult>(string sql, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null) where TDataResult : class
        {
            var connectionString = _dataConnections.CsPortalDb;

            var connection = new SqlConnection(connectionString);
            var command = connection.CreateCommand();

            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (sqlParameters != null && sqlParameters.Any())
            {
                command.Parameters.AddRange(sqlParameters);
            }
            try
            {
                connection.Open();
                var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                var result = dataMappingAction(reader);

                reader.Close();

                return result;
            }
            finally
            {
                command.Dispose();
                connection.Close();
            }
        }

        public async Task<TDataResult> GetDataByStoredProcedureAsync<TDataResult>(string storedProcedureName, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null, int? timeoutInSeconds = null, AppDbConnectionName? connectionName = AppDbConnectionName.CsPortalDb) where TDataResult : class
        {
            var connectionString = _dataConnections.CsPortalDb;
            if (connectionName == AppDbConnectionName.SecondaryCsPortalDb && !string.IsNullOrEmpty(_dataConnections.SecondaryCsPortalDb))
            {
                connectionString = _dataConnections.SecondaryCsPortalDb;
            }

            if (timeoutInSeconds.HasValue)
            {
                connectionString += $";Connect Timeout={timeoutInSeconds.Value};";
            }

            var connection = new SqlConnection(connectionString);
            var command = connection.CreateCommand();

            command.CommandText = storedProcedureName;
            command.CommandType = CommandType.StoredProcedure;

            if (sqlParameters != null && sqlParameters.Any())
            {
                if (timeoutInSeconds.HasValue)
                {
                    command.CommandTimeout = timeoutInSeconds.Value;
                }
                command.Parameters.AddRange(sqlParameters);
            }
            try
            {
                connection.Open();
                var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                var result = dataMappingAction(reader);

                reader.Close();

                return result;
            }
            finally
            {
                command.Dispose();
                connection.Close();
            }
        }

        public string GetValueFromVariable(string sql, SqlParameter[] sqlParameters)
        {
            var resultParam = new SqlParameter()
            {
                ParameterName = "@result",
                SqlDbType = SqlDbType.NVarChar,
                Size = 512,
                Direction = ParameterDirection.Output
            };

            var connectionString = _dataConnections.CsPortalDb;

            var connection = new SqlConnection(connectionString);
            var command = connection.CreateCommand();

            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.Parameters.Add(resultParam);

            if (sqlParameters != null && sqlParameters.Any())
            {
                command.Parameters.AddRange(sqlParameters);
            }
            try
            {
                connection.Open();
                var result = command.ExecuteNonQuery();
                return resultParam.Value.ToString();
            }
            finally
            {
                command.Dispose();
                connection.Close();
            }
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            try
            {
                var result = _dbContext.Database.ExecuteSqlRaw(sql, parameters);
                return result;
            }
            catch (ObjectDisposedException ex)
            {
                var connectionString = _dataConnections.CsPortalDb;
                var optionsBuilder = new DbContextOptionsBuilder<SpContext>();
                optionsBuilder.UseSqlServer(connectionString);
                using (SpContext dbContext = new SpContext(optionsBuilder.Options))
                {
                    var result = dbContext.Database.ExecuteSqlRaw(sql, parameters);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<DataSourceResult> ToDataSourceResultAsSingleQueryAsync<TQueryModel, TModel>(IQueryable<TQueryModel> query, DataSourceRequest request, Func<TQueryModel, TModel> dataMapping = null) where TQueryModel : DataSourceSingleQueryModel
        {
            var dataSourceQuery = query.GetDataSouceQueryable(request);
            var dataSurceQuerySqlString = dataSourceQuery.ToQueryString();
            var regex1 = @"\[\w*\].\[TotalRows\]";
            var finalDataSurceQuerySqlString = Regex.Replace(dataSurceQuerySqlString, regex1, "COUNT(*) OVER() AS [TotalRows]");
            var textIndex = finalDataSurceQuerySqlString.LastIndexOf("ROWS ONLY");
            finalDataSurceQuerySqlString = finalDataSurceQuerySqlString.Substring(0, textIndex + 9);
            var returnedData = await GetQueryable<TQueryModel>(finalDataSurceQuerySqlString).ToListAsync();

            var result = new DataSourceResult();

            if (returnedData == null || !returnedData.Any())
            {
                result.Total = 0;
                result.Data = null;
            }
            else
            {
                var firstRow = returnedData.First();
                result.Total = firstRow.TotalRows;
            }
            

            // Call data mapping to transform model type if needed
            if (dataMapping == null)
            {
                result.Data = returnedData;
            }
            else
            {

                var destinationData = new List<TModel>();
                foreach (var row in returnedData)
                {
                    var item = dataMapping(row);
                    destinationData.Add(item);
                }
                result.Data = destinationData;
            }

            return result;
        }
    }
}
