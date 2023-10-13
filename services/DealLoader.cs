using MainApp.common;
using MainApp.models;

namespace MainApp.services
{
    public class DealLoader : IDealLoader
    {
        private readonly IDataLoader dataSource;
        private string header = string.Empty;

        public DealLoader(IDataLoader dataSource)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }

        

        public async IAsyncEnumerable<Deal> LoadDeals()
        {
            var dealIds = new HashSet<int>();
            await foreach (var line in dataSource.LoadData())
            {

                if (dataSource.CurrentLineNumber == 1)
                {
                    header = line;
                    continue;
                }

                var values = line.Split(',');

                if (values.Length != 4)
                {
                    throw new InvalidDataException($"Invalid data at line {dataSource.CurrentLineNumber}");
                }

                if (!int.TryParse(values[0], out var dealId))
                {
                    if (header == line) continue;

                    throw new InvalidDataException($"Invalid deal id at line {dataSource.CurrentLineNumber}");
                }

                if (!dealIds.Add(dealId))
                {
                    throw new InvalidDataException($"Duplicate deal id at line {dataSource.CurrentLineNumber}");
                }

                var company = values[1];
                var peril = values[2];
                var location = values[3];

                yield return new Deal(dealId, company, peril, location);
            }
        }

    }
}


