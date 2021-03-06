using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using HBHplayground.Abstractions;

namespace HBHplayground.Server.Services
{
    [ComputeService(typeof(ITimeService))]
    public class TimeService : ITimeService
    {
        private DateTime _startTime = DateTime.UtcNow;

        [ComputeMethod(AutoInvalidateTime = 0.25, KeepAliveTime = 1)]
        public virtual async Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default)
        {
            var time = DateTime.Now;
            if (time.Second % 10 == 0)
                // This delay is here solely to let you see ServerTime page in
                // in "Loading" / "Updating" state.
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            return time;
        }

        public virtual Task<TimeSpan> GetUptimeAsync(TimeSpan updatePeriod, CancellationToken cancellationToken = default)
        {
            var computed = Computed.GetCurrent();
            var time = DateTime.UtcNow;
            time=time.AddYears(100);
            //time.Days += 42;
            Task.Delay(updatePeriod, default)
                .ContinueWith(_ => computed!.Invalidate(), CancellationToken.None);
            return Task.FromResult(time- _startTime );
        }
    }
}
