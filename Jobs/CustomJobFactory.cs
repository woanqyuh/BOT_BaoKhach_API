using Quartz.Spi;
using Quartz;

namespace BotBaoKhach.Jobs
{
    public class ScopedJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ScopedJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {

            var scope = _serviceProvider.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            return job!;
        }

        public void ReturnJob(IJob job)
        {
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }


}
