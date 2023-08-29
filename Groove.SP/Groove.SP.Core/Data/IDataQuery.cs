using Groove.SP.Core.Models;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Core.Data
{
    public interface IDataQuery
    {
        /// <summary>
        /// Creates a LINQ query for the query type based on a raw SQL query.
        /// </summary>
        /// <typeparam name="TQuery">Query type</typeparam>
        /// <param name="sql">The raw SQL query</param>
        /// <returns>An IQueryable representing the raw SQL query</returns>
        IQueryable<T> GetQueryable<T>(string sql, params object[] parameters) where T : class;

        /// <summary>
        /// To get data from specific SQL statement.
        /// <br/>
        /// <b>It is executed via ADO.NET and ISOLATED to current EF database context.</b>
        /// </summary>
        /// <typeparam name="TDataResult"></typeparam>
        /// <param name="sql">SQL statement</param>
        /// <param name="dataMappingAction">How to mapping data returned from SQL to model</param>
        /// <param name="sqlParameters">Input parameters</param>
        /// <returns></returns>
        TDataResult GetDataBySql<TDataResult>(string sql, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null) where TDataResult : class;

        /// <summary>
        /// To get data from specific SQL Stored Procedure.
        /// <br/>
        /// <b>It is executed via ADO.NET and ISOLATED to current EF database context.</b>
        /// </summary>
        /// <typeparam name="TDataResult"></typeparam>
        /// <param name="storedProcedureName">SQL Stored Procedure name</param>
        /// <param name="dataMappingAction">How to mapping data returned from SQL to model</param>
        /// <param name="sqlParameters">Input parameters</param>
        /// <param name="timeoutInSeconds">Number of seconds for timeout (connection and command). Default value is from connection string (30 seconds).</param>
        /// <param name="connectionName">Name of database connection string to use</param>
        /// <returns></returns>
        Task<TDataResult> GetDataByStoredProcedureAsync<TDataResult>(string storedProcedureName, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null, int? timeoutInSeconds = 30, AppDbConnectionName? connectionName = AppDbConnectionName.CsPortalDb) where TDataResult : class;

        /// <summary>
        /// To get data from specific SQL statement with output variable returned
        /// <br/>
        /// <b>It is executed via ADO.NET and ISOLATED to current EF database context.</b>
        /// </summary>
        /// <param name="sql">SQL statement</param>
        /// <param name="sqlParameters">Input parameters</param>
        /// <returns></returns>
        string GetValueFromVariable(string sql, SqlParameter[] sqlParameters);

        /// <summary>
        /// To execute SQL query to database.<br/>
        /// <b>It will use current EF database object to execute and create a new instance automatically if the current one was disposed.</b>
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteSqlCommand(string sql, params object[] parameters);

        /// <summary>
        /// To get data source result for grid/list by single query to database.
        /// </summary>
        /// <typeparam name="TQueryModel">Query model that must contains column [TotalRows].
        /// <br></br>See more at <see cref="Groove.SP.Core.Models.DataSourceSingleQueryModel"/>
        /// </typeparam>
        /// <typeparam name="TModel">Data model of data source result</typeparam>
        /// <param name="query">Query</param>
        /// <param name="request">Data source quest</param>
        /// <param name="dataMapping">Data mapping rules from TQueryModel to TModeal. If null, use TQueryModel</param>
        /// <returns></returns>
        Task<DataSourceResult> ToDataSourceResultAsSingleQueryAsync<TQueryModel, TModel>(IQueryable<TQueryModel> query, DataSourceRequest request, Func<TQueryModel, TModel> dataMapping) where TQueryModel : DataSourceSingleQueryModel;
    }
}
