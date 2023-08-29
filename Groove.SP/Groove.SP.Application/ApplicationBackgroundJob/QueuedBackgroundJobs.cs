using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.ApplicationBackgroundJob.ViewModels;
using Groove.SP.Application.Common;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.ApplicationBackgroundJob
{
    /// <summary>
    /// All jobs enqueued will be fired automatically after action executed successfully on controller.
    /// Use it to assure background jobs executed after current process done successfully.
    /// </summary>
    public class QueuedBackgroundJobs: IQueuedBackgroundJobs
    {
        private readonly ConcurrentQueue<Job> _enqueuedJobQueue;
        private readonly ConcurrentQueue<AppScheduledJobViewModel> _scheduledJobQueue;

        private readonly IBackgroundJobClient _client;

        public QueuedBackgroundJobs(IBackgroundJobClient client)
        {
            _enqueuedJobQueue = new ConcurrentQueue<Job>();
            _scheduledJobQueue = new ConcurrentQueue<AppScheduledJobViewModel>();
            _client = client;
        }

        /// <summary>
        /// Add action to the queue and execute later
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            var newJob = Job.FromExpression(methodCall);
            _enqueuedJobQueue.Enqueue(newJob);            
        }

        /// <summary>
        /// Add action to the queue and schedule later
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodCall"></param>
        public void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
        {
            var newJob = Job.FromExpression(methodCall);
            var scheduledJob = new AppScheduledJobViewModel
            {
                Delay = delay,
                Job = newJob
            };
            _scheduledJobQueue.Enqueue(scheduledJob);
        }

        /// <summary>
        /// Execute jobs that were added to the queue for enqueued and scheduled
        /// </summary>
        public void FireAllJobs()
        {
            while (!_enqueuedJobQueue.IsEmpty)
            {
                if (_enqueuedJobQueue.TryDequeue(out var job))
                {
                    _client.Create(job, new EnqueuedState());
                }
            }

            while (!_scheduledJobQueue.IsEmpty)
            {
                if (_scheduledJobQueue.TryDequeue(out var scheduledJob))
                {
                    _client.Create(scheduledJob.Job, new ScheduledState(scheduledJob.Delay));
                }
            }
        }
    }
}
