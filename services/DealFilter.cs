using MainApp.common;
using MainApp.models;
using System.Runtime.CompilerServices;


namespace MainApp.services
{

    public class DealFilter : IDealFilter
    {
        public async IAsyncEnumerable<Deal> FilterDeals(IAsyncEnumerable<Deal> deals, models.Contract contract, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            
            if (deals == null || contract == null)
            {

                await foreach (var deal in Enumerable.Empty<Deal>().ToAsyncEnumerable().WithCancellation(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return deal;

                }
                yield break;
            }
            var dealCount = await deals.CountAsync(cancellationToken: cancellationToken);

            if (dealCount <= 0)
            {

                await foreach (var deal in Enumerable.Empty<Deal>().ToAsyncEnumerable().WithCancellation(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return deal;

                }
                yield break;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var locationCoverage = contract?.Coverage?.Where(c => c.Attribute == "Location").SelectMany(c => c.Include).ToHashSet();

            var perilCoverage = contract?.Coverage?.Where(c => c.Attribute == "Peril").SelectMany(c => c.Exclude).ToHashSet();

            if (locationCoverage == null || perilCoverage == null)
            {
                yield break;
            }

            await foreach (var deal in deals.WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (deal.Amount <= contract?.MaxAmount &&
                    locationCoverage.Contains(deal.Location) &&
                    !perilCoverage.Contains(deal.Peril))
                {
                    yield return deal;
                }
            }

        }


    }
}
