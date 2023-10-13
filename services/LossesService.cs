using MainApp.common;
using MainApp.models;
using Newtonsoft.Json;
using System.Text;

namespace MainApp.services
{
    public class LossesService : ILossesService
    {
        private IDealLoader DealLoader { get; set; }
        private IDealFilter DealFilter { get; set; }
        private IContractLoader ContractLoader { get; set; }
        private IStreamReaderFactory StreamReaderFactory { get; set; }

        private const int CsvFieldCount = 3;

        public LossesService(IDealLoader dealLoader, IDealFilter dealFilter, IContractLoader contractLoader, IStreamReaderFactory streamReaderFactory)
        {
            DealLoader = dealLoader;
            DealFilter = dealFilter;
            ContractLoader = contractLoader;
            StreamReaderFactory = streamReaderFactory;
        }

        public async IAsyncEnumerable<EventData> ReadEventData(string csvFilePath)
        {
            CheckParameters(csvFilePath);

            using var reader = new StreamReader(csvFilePath);
            // Skip header row if present
            if (!reader.EndOfStream)
            {
                await reader.ReadLineAsync();
            }

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null)
                {
                    var fields = line.Split(',');

                    if (fields != null && fields.Length == 3 &&
                        int.TryParse(fields[0], out var eventId) &&
                        int.TryParse(fields[1], out var dealId) &&
                        int.TryParse(fields[2], out var loss))
                    {
                        yield return new EventData
                        {
                            EventId = eventId,
                            DealId = dealId,
                            Loss = loss
                        };
                    }
                }
            }
        }


        private static void CheckParameters(string csvFilePath)
        {
            if (csvFilePath == null)
            {
                throw new ArgumentNullException(nameof(csvFilePath));
            }

            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException();
            }
        }

        public async IAsyncEnumerable<Loss> CalculateLosses(string lossesPath, string contractsPath)
        {
            // Read in the losses.csv file
            var lossLines = ReadEventData(lossesPath);

            var dealList = DealLoader.LoadDeals();

            await using var fileStream = new FileStream(contractsPath, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096, true);
            await using var jsonTextReader = new JsonTextReader(streamReader);
            var contract = new ContractLoader(() => jsonTextReader).LoadContract();
            var coveredDeals = new DealFilter().FilterDeals(dealList, contract);

            // Create lookup for covered deals for fast access
            var coveredDealsLookup = await coveredDeals.ToLookupAsync(d => d.DealId);

            await foreach (var eventData in lossLines)
            {
                if (coveredDealsLookup.Contains(eventData.DealId))
                {
                    var deal = coveredDealsLookup[eventData.DealId].First();
                    yield return new Loss { Peril = deal.Peril, Amount = eventData.Loss };
                }
            }
        }        
    }
}
