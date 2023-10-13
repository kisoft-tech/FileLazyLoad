using MainApp.models;
using System.Runtime.CompilerServices;

namespace MainApp.common
{
    public interface IDealFilter
    {
        IAsyncEnumerable<Deal> FilterDeals(IAsyncEnumerable<Deal> deals, Contract contract, [EnumeratorCancellation] CancellationToken cancellationToken = default);
    }
}