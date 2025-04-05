using Microsoft.Extensions.Logging;
using Quartz;
//using Quartz.Plugin.TimeZoneConverter;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace selfTriggerTest5.Jobs
{
    public class ScheduledJob : IJob
    {
        private readonly ILogger<ScheduledJob> _logger;
        private readonly HttpClient _httpClient;

        public ScheduledJob(ILogger<ScheduledJob> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var executionTimeUtc = DateTime.UtcNow;

            // Convert UTC execution time to Sri Lanka Time (UTC+5:30)
            TimeZoneInfo sriLankaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
            var executionTimeSL = TimeZoneInfo.ConvertTimeFromUtc(executionTimeUtc, sriLankaTimeZone);

            // Retrieve the message stored in job data
            var jobMessage = context.MergedJobDataMap.GetString("Message") ?? "No message provided";

            _logger.LogInformation($"🚀 Job triggered at {executionTimeSL} (SLST) [UTC: {executionTimeUtc}] - Message: {jobMessage}");
            Console.WriteLine($"🚀 Job triggered at {executionTimeSL} (SLST) [UTC: {executionTimeUtc}] - Message: {jobMessage}");

            // Example: Trigger an API call
            //var apiUrl = "https://localhost:7138/WeatherForecast";
            var apiUrl = "https://localhost:7215/WeatherForecast";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API response body: {responseBody}");
            }
            else
            {
                _logger.LogError($"API call failed with status code {response.StatusCode}.");
            }
        }
    }

}