using BotBaoKhach.Dtos;
using BotBaoKhach.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Quartz;
using Quartz.Impl.Matchers;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace BotBaoKhach.Jobs
{
    public class JobScheduler
    {
        private readonly IScheduler _scheduler;
        private readonly IConfiguration _configuration;
        public JobScheduler(IScheduler scheduler, IConfiguration configuration)
        {
            _scheduler = scheduler;
            _configuration = configuration;
        }


        public async Task ScheduleJobs(SettingBaoKhachDto thietlap)
        {
            if (thietlap == null)
            {
                Console.WriteLine("Invalid setting: thietlap is null.");
                return;
            }

            var jobId = thietlap.Id.ToString();
            var jobKey = new JobKey(jobId, "SettingBaoKhach");

            // Kiểm tra nếu job đã tồn tại, tránh trùng lặp
            if (await _scheduler.CheckExists(jobKey))
            {
                Console.WriteLine($"Job {jobId} already exists. Skipping scheduling.");
                return;
            }

            var jobDataMap = new JobDataMap{{ "SettingId", jobId } };

            IJobDetail job = JobBuilder.Create<Job>()
                .WithIdentity(jobKey)
                .UsingJobData(jobDataMap)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{jobId}-trigger", "SettingBaoKhach")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(15)
                .RepeatForever())
            .Build();

            try
            {
                var domainWebhook = _configuration.GetValue<string>("AdminAccount:Webhook");

                if (!string.IsNullOrWhiteSpace(thietlap.BotToken))
                {
                    var botClient = new TelegramBotClient(thietlap.BotToken);
                    await botClient.DeleteWebhook();
                    await botClient.GetUpdates(offset: -1);
                    await botClient.SetWebhook(
                        url: $"{domainWebhook}/{thietlap.BotToken}",
                        allowedUpdates: new[] { UpdateType.Message }
                    );
                    Console.WriteLine($"Webhook registered for bot {thietlap.BotToken}");
                    await _scheduler.ScheduleJob(job, trigger);
                    Console.WriteLine($"Scheduled job {jobId} successfully.");
                }
                else
                {
                    Console.WriteLine("BotToken is missing, skipping webhook registration and cancel scheduled job");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scheduling job {jobId}: {ex.Message}");

            }
        }

        public async Task RescheduleJobs(SettingBaoKhachDto thietlap)
        {
            await DeleteScheduledJobs(thietlap);
            await ScheduleJobs(thietlap);
        }
        public async Task DeleteScheduledJobs(SettingBaoKhachDto thietlap)
        {
            var jobKey = new JobKey(thietlap.Id.ToString(), "SettingBaoKhach");

            if (await _scheduler.CheckExists(jobKey))
            {
                if (!string.IsNullOrWhiteSpace(thietlap.BotToken))
                {
                    try
                    {
                        var botClient = new TelegramBotClient(thietlap.BotToken);
                        await botClient.DeleteWebhook();
                        Console.WriteLine($"Deleted webhook for bot {thietlap.BotToken}");
                        await _scheduler.DeleteJob(jobKey);
                        Console.WriteLine($"Deleted job: {jobKey.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting webhook for bot {thietlap.BotToken}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"No valid bot token found for job {thietlap.Id}, skipping webhook deletion.");
                }
            }
            else
            {
                Console.WriteLine($"Job {jobKey.Name} does not exist.");
            }
        }
    }
}
