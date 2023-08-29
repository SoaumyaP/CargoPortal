using Groove.SP.Core.Models;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace Groove.SP.Application.HangfireJob
{
    public class RemoveFailedJob
    {
        private readonly ILogger<RemoveFailedJob> _logger;
        private readonly AppDbConnections _dataConnections;

        public RemoveFailedJob(ILogger<RemoveFailedJob> logger, AppDbConnections dataConnections)
        {
            _logger = logger;
            _dataConnections = dataConnections;
        }

        [JobDisplayName("Hangfire - Remove failed jobs")]
        public void Execute()
        {
            if (!string.IsNullOrEmpty(_dataConnections.HangfireDb))
            {
                // 21 600 seconds = 6 hours
                var timeout = 21600;
                _logger.LogInformation("Remove Hangfire Failed Job Starting...");
                var dbConnectionString = _dataConnections.HangfireDb;
                dbConnectionString += $";Connect Timeout={timeout};";
                var connection = new SqlConnection(dbConnectionString);
                var command = connection.CreateCommand();
                command.CommandTimeout = timeout;

                command.CommandText = @"
                                        DECLARE @RowCount BIGINT;

                                        BEGIN TRANSACTION

                                            DELETE 
                                            FROM HangFire.Job
                                            WHERE StateName = 'Failed' AND CreatedAt <= CONCAT(CONVERT(CHAR(10), DATEADD(year, -1, GETUTCDATE()), 120),' 23:59:59.999')

                                            SET @RowCount = @@ROWCOUNT
                                            IF @RowCount > 0 
                                            BEGIN 
		                                        UPDATE [HangFire].[AggregatedCounter]
		                                        SET Value = Value + @RowCount
		                                        WHERE [Key] = 'stats:deleted'
                                            END;

                                        COMMIT TRANSACTION";

                command.CommandType = CommandType.Text;

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
                _logger.LogInformation("Remove Hangfire Failed Job Completed.");
            }
        }
    }
}
