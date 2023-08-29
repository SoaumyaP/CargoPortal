using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;

namespace Groove.SP.Application.Utilities
{
    /// <summary>
    /// Mark Hangfire to expire/delete job in 30 minutes (as default) after success
    /// </summary>
    public class ShortExpirationJobAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private int _retentionTimeInMinutes { get; set; }

        public ShortExpirationJobAttribute() : this(30)
        {
        }

        public ShortExpirationJobAttribute(int retentionTimeInMinutes)
        {
            // Constraint, value must be > 0
            _retentionTimeInMinutes = retentionTimeInMinutes > 0 ? retentionTimeInMinutes : 30;
        }
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromMinutes(_retentionTimeInMinutes);
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromMinutes(_retentionTimeInMinutes);
        }
    }
}
