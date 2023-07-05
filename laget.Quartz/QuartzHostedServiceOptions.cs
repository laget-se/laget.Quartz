﻿using System;

namespace laget.Quartz
{
    public class QuartzHostedServiceOptions
    {
        /// <summary>
        /// If true (default), jobs will not be started until application startup completes.
        /// This avoids the running of jobs <em>during</em> application startup.
        /// </summary>
        public bool AwaitApplicationStarted { get; set; } = true;

        public bool RegisterDefaultJobListener { get; set; } = true;

        public bool RegisterDefaultSchedulerListener { get; set; } = false;

        public bool RegisterDefaultTriggerListener { get; set; } = false;

        public bool ScanAndRegisterJobs { get; set; } = true;

        /// <summary>
        /// <para>
        /// If not <see langword="null" /> the scheduler will start after specified delay.
        /// </para>
        /// <para>
        /// If <see cref="AwaitApplicationStarted"/> is true, the delay starts when application startup completes.
        /// </para>
        /// </summary>
        public TimeSpan? StartDelay { get; set; }

        /// <summary>
        /// If <see langword="true" /> the scheduler will not allow shutdown process
        /// to return until all currently executing jobs have completed.
        /// </summary>
        public bool WaitForJobsToComplete { get; set; }
    }
}
