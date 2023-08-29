using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Infrastructure.Hangfire
{
    public class LongTimeRetentionJobAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private int _retentionTimeInDay { get; set; }

        public LongTimeRetentionJobAttribute(int retentionTimeInDay)
        {
            // Constraint, value must be > 0
            _retentionTimeInDay = retentionTimeInDay > 0 ? retentionTimeInDay : 1;
        }
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(_retentionTimeInDay);
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(_retentionTimeInDay);
        }
    }
}
