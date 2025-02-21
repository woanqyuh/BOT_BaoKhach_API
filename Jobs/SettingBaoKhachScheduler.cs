using BotBaoKhach.Repositories;
using Quartz.Impl;
using Quartz;
using BotBaoKhach.Repositories;

namespace BotBaoKhach.Jobs
{
    public class SettingBaoKhachScheduler
    {
        private readonly ISettingBaoKhachRepository _repository;
        private readonly JobScheduler _scheduler;

        public SettingBaoKhachScheduler(ISettingBaoKhachRepository repository, JobScheduler scheduler)
        {
            _repository = repository;
            _scheduler = scheduler;
        }

        public async Task Start()
        {

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();


            var activeRecords = await _repository.GetActiveRecordsAsync();


            if (activeRecords.Any())
            {
                Console.WriteLine("Starting to scan records and schedule jobs...");

                foreach (var record in activeRecords)
                {

                    await _scheduler.ScheduleJobs(record);
                }

                Console.WriteLine("All jobs have been scheduled.");
            }
            else
            {
                Console.WriteLine("No records found to schedule.");
            }

            Console.WriteLine("Scheduler has started.");
        }

    }
}
