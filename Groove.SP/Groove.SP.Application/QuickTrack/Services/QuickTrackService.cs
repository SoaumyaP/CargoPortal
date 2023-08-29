using Groove.SP.Core.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Groove.SP.Application.QuickTrack.Services.Interfaces;
using Groove.SP.Application.QuickTrack.ViewModels;

namespace Groove.SP.Application.QuickTrack.Services
{
    public class QuickTrackService : IQuickTrackService
    {
        private readonly IDataQuery _dataQuery;

        public QuickTrackService(IDataQuery dataQuery)
        {
            _dataQuery = dataQuery;
        }

        public async Task<QuickTrackOptionResult> GetQuickTrackOptionAsync(string searchTerm, bool isInternal, long organizationId = 0, string affiliates = "", string supplierCustomerRelationships = "")
        {
            if (!string.IsNullOrWhiteSpace(affiliates)) affiliates = affiliates.Replace("[", string.Empty).Replace("]", string.Empty).Trim();

            var storedProcedureName = "spu_GetQuickTrackOption";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@searchTerm",
                        Value = searchTerm,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@isInternal",
                        Value = isInternal,
                        DbType = DbType.Boolean,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@organizationId",
                        Value = organizationId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@supplierCustomerRelationships",
                        Value = supplierCustomerRelationships,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, QuickTrackOptionResult> mapping = (reader) =>
            {
                var mappedData = new QuickTrackOptionResult();

                while (reader.Read())
                {
                    object tmpValue;

                    // Must be in order of data reader
                    mappedData.MatchedOption = reader[0].ToString();

                    tmpValue = reader[1];
                    mappedData.MatchedValue = DBNull.Value == tmpValue ? null : (long)tmpValue;

                    break;
                }

                return mappedData;
            };

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());

            return result;
        }
    }
}
