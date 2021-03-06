using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HBHplayground.Abstractions;
using Stl.Fusion;

namespace HBHplayground.Server.Services
{
    [ComputeService(typeof(ISumService))]
    public class SumService : ISumService
    {
        private readonly IMutableState<double> _accumulator;

        public SumService(IStateFactory stateFactory)
            => _accumulator = stateFactory.NewMutable<double>();

        public Task ResetAsync(CancellationToken cancellationToken)
        {
            _accumulator.Value = 0;
            return Task.CompletedTask;
        }

        public Task AccumulateAsync(double value, CancellationToken cancellationToken)
        {
            _accumulator.Value += value;
            return Task.CompletedTask;
        }

        // Compute methods

        public virtual async Task<double> GetAccumulatorAsync(CancellationToken cancellationToken)
            => await _accumulator.UseAsync(cancellationToken);

        public virtual async Task<double> SumAsync(double[] values, bool addAccumulator, CancellationToken cancellationToken)
        {
            var sum = values.Sum();
            if (addAccumulator)
                sum += await GetAccumulatorAsync(cancellationToken);
            return sum;
        }
    }
}
