using System;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace HBHplayground.Abstractions
{
    public interface ITimeService
    {
        [ComputeMethod(KeepAliveTime = 1)]
        Task<DateTime> GetTimeAsync(CancellationToken cancellationToken = default);
        [ComputeMethod(KeepAliveTime = 60)]
        Task<TimeSpan> GetUptimeAsync(TimeSpan updatePeriod, CancellationToken cancellationToken = default);
    }
}
