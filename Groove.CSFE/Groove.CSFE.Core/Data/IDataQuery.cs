using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Core.Data
{
    public interface IDataQuery
    {
        IQueryable<T> GetQueryable<T>(string sql, params object[] parameters) where T : class;

        TDataResult GetDataBySql<TDataResult>(string sql, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null) where TDataResult : class;

        string GetValueFromVariable(string sql, SqlParameter[] sqlParameters);

        Task<TDataResult> GetDataByStoredProcedureAsync<TDataResult>(string storedProcedureName, Func<DbDataReader, TDataResult> dataMappingAction, SqlParameter[] sqlParameters = null) where TDataResult : class;

        /// <summary>
        /// To insert large amount of data with table-value parameter type
        /// </summary>
        /// <param name="sql">Insert sql statement</param>
        /// <param name="dtTable">Data-table that contains data</param>
        /// <param name="customDataTypeName">Name of table-value parameter type</param>
        /// <returns>A number of records that were inserted</returns>
        Task<object> BulkInsertDataWithCustomDataTypeAsync(string sql, DataTable dtTable, string customDataTypeName);

    }
}
