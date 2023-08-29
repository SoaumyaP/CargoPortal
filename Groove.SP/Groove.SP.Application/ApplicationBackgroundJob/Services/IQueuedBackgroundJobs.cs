using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.ApplicationBackgroundJob.Services
{
    /// <summary>
    /// All jobs enqueued will be fired automatically after action executed successfully on controller.
    /// Use it to assure background jobs executed after current process done successfully.
    /// </summary>
    public interface IQueuedBackgroundJobs
    {
        /// <summary>
        /// Add action to the queue and execute later
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        void Enqueue<T>(Expression<Func<T, Task>> methodCall);


        /// <summary>
        /// Add action to the queue and schedule later
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

        /// <summary>
        /// Execute jobs that were added to the queue for enqueued and scheduled
        /// </summary>
        void FireAllJobs();
    }
}
