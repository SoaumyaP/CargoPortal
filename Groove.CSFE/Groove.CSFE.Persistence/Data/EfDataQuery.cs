using Groove.CSFE.Core.Data;
using Groove.CSFE.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Persistence.Data
{
    public class EfDataQuery : IDataQuery
    {
        private readonly DbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;

        public EfDataQuery(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _dbContext = (DbContext)_serviceProvider.GetService(typeof(CsfeContext));
        }

        /// <summary>
        /// Creates a LINQ query for the query type based on a raw SQL query
        /// </summary>
        /// <typeparam name="TQuery">Query type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        public IQueryable<TQuery> GetQueryable<TQuery>(string sql, params object[] parameters) where TQuery : class
        {
            return _dbContext.Set<TQuery>().FromSqlRaw(sql, parameters);
        }

        public TDataResult GetDataBySql<TDataResult>(string sql, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null) where TDataResult : class
        {
            var connectionString = _dbContext.Database.GetConnectionString();

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

        /// <summary>
        /// To get data from specific SQL statement with output variable returned
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="sqlParameters">Input parameters</param>
        /// <returns></returns>
        public string GetValueFromVariable(string sql, SqlParameter[] sqlParameters)
        {
            var resultParam = new SqlParameter()
            {
                ParameterName = "@result",
                SqlDbType = SqlDbType.VarChar,
                Size = 512,
                Direction = ParameterDirection.Output
            };

            var connectionString = _dbContext.Database.GetConnectionString();

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

        /// <summary>
        /// To get data from specific SQL Stored Procedure. It is executed via ADO.NET with connection by current EF database context.
        /// </summary>
        /// <typeparam name="TDataResult"></typeparam>
        /// <param name="storedProcedureName">SQL Stored Procedure name</param>
        /// <param name="dataMappingAction">How to mapping data returned from SQL to model</param>
        /// <param name="sqlParameters">Input parameters</param>
        /// <returns></returns>
        public async Task<TDataResult> GetDataByStoredProcedureAsync<TDataResult>(string storedProcedureName, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null) where TDataResult : class
        {
            var connectionString = _dbContext.Database.GetConnectionString();

            var connection = new SqlConnection(connectionString);
            var command = connection.CreateCommand();

            command.CommandText = storedProcedureName;
            command.CommandType = CommandType.StoredProcedure;

            if (sqlParameters != null && sqlParameters.Any())
            {
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

        public async Task<object> BulkInsertDataWithCustomDataTypeAsync(string sql, DataTable dtTable, string customDataTypeName)
        {
            var connectionString = _dbContext.Database.GetConnectionString();

            var connection = new SqlConnection(connectionString);
            var command = connection.CreateCommand();

            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            var param = command.Parameters.AddWithValue("@importDataTable", dtTable);
            param.TypeName = customDataTypeName;
           
            try
            {
                Console.WriteLine(DateTime.Now);
                connection.Open();
                object result = await command.ExecuteScalarAsync();
                return result;
                Console.WriteLine(DateTime.Now);

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                command.Dispose();
                connection.Close();
            }
        }
    }
}
