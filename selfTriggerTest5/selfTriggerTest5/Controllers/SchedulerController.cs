using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl.Matchers;
using System;
//using Quartz.Plugin.TimeZoneConverter;
using selfTriggerTest5.Jobs;

namespace selfTriggerTest5.Controllers
{
    [ApiController]
    [Route("api/scheduler")]
    public class SchedulerController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public SchedulerController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        [HttpPost("schedule-job")]
        public async Task<IActionResult> ScheduleJob([FromBody] ScheduleJobRequest request)
        {
            // Define Sri Lanka's timezone (UTC+5:30)
            TimeZoneInfo sriLankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");

            // Ensure TriggerTime is properly set
            DateTime triggerTimeLocal = DateTime.SpecifyKind(request.TriggerTime, DateTimeKind.Unspecified);

            // Convert the provided time (Sri Lanka Time) to UTC
            var triggerTimeUtc = TimeZoneInfo.ConvertTimeToUtc(triggerTimeLocal, sriLankaTimeZone);

            var scheduler = await _schedulerFactory.GetScheduler();

            // Generate unique job identity to allow multiple jobs
            var jobId = Guid.NewGuid().ToString();
            var job = JobBuilder.Create<ScheduledJob>()
                .WithIdentity($"Job_{jobId}", "Group1") // Unique job name
                .UsingJobData("Message", request.Message) // Store custom message
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"Trigger_{jobId}", "Group1") // Unique trigger name
                .StartAt(triggerTimeUtc) // Schedule in UTC
                .Build();

            await scheduler.ScheduleJob(job, trigger);

            return Ok($"Job scheduled at {request.TriggerTime} (SLST) → {triggerTimeUtc} UTC with message: {request.Message}");
        }


        [HttpGet("get-all-jobs")]
        public async Task<IActionResult> GetAllJobs()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

            if (jobKeys.Count == 0)
                return Ok("No scheduled jobs.");

            var jobDetails = new List<string>();

            foreach (var jobKey in jobKeys)
            {
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                foreach (var trigger in triggers)
                {
                    jobDetails.Add($"Job: {jobKey.Name}, Next Run: {trigger.GetNextFireTimeUtc()}, Time Now: {DateTime.UtcNow}");
                }
            }

            return Ok(jobDetails);
        }

    }

    // DTO for request body
    public class ScheduleJobRequest
    {
        public DateTime TriggerTime { get; set; }
        public string Message { get; set; }
    }
}
