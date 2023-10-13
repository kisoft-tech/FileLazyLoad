using MainApp.common;
using MainApp.models;
using MainApp.services;
using Newtonsoft.Json;
using System.Text;

namespace MainApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            try
            {
                await using var fileStream = new FileStream("contract.json", FileMode.Open, FileAccess.Read);
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096, true);

                var contractLoader = new ContractLoader(() => new JsonTextReader(streamReader));
                var contract = contractLoader.LoadContract();


                var ds = new FileDataLoader("deals.csv", new FileWrapper(), new FileStreamProvider());
                var deals = new DealLoader(ds).LoadDeals();
                var coveredDeals = new DealFilter().FilterDeals(deals, contract);

                // Write the filtered deals to an output file in the specified format
                await using var writer = new StreamWriter("filteredDeals.csv");

                await writer.WriteLineAsync("DealId,Company,Peril,Location");
                Console.WriteLine("Writing filtered deals to file...");
                Console.WriteLine("DealId,Company,Peril,Location");
                await foreach (var deal in coveredDeals)
                {
                    await writer.WriteLineAsync($"{deal.DealId},{deal.Company},{deal.Peril},{deal.Location}");
                    Console.WriteLine($"DealId: {deal.DealId}, Company: {deal.Company}, Peril: {deal.Peril}, Location: {deal.Location}");
                }
                await writer.FlushAsync();
                writer.Close();
                var ds2 = new FileDataLoader("deals.csv", new FileWrapper(), new FileStreamProvider());
                
                IDealFilter dealFilter = new DealFilter();
                IStreamReaderFactory streamReaderFactory = new StreamReaderFactory();
                var losses = new LossesService(new DealLoader(ds2), dealFilter, contractLoader, streamReaderFactory).CalculateLosses("losses.csv", "contract.json");

                var groupedLosses = losses.GroupBy(l => l.Peril)
                                      .Select(g => new { Peril = g.Key, Loss = g.SumAsync(l => l.Amount) })
                                      ;

                
                var count = await groupedLosses.CountAsync();
                // Write the filtered deals to an output file in the specified format
                await using var writer2 = new StreamWriter("totalLoss.csv");
                await writer2.WriteLineAsync("Peril,Loss");
                Console.WriteLine("Writing total losses to file...");
                Console.WriteLine("Peril,Loss");
                for (var i = 0; i < count; i++)
                {
                    await writer2.WriteLineAsync($"{groupedLosses.ElementAtAsync(i).Result.Peril},{groupedLosses.ElementAtAsync(i).Result.Loss}");
                    Console.WriteLine($"{groupedLosses.ElementAtAsync(i).Result.Peril},{groupedLosses.ElementAtAsync(i).Result.Loss}");
                }
                Console.WriteLine("Finished.");
                Console.WriteLine("Press any key to close app.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            catch (Exception e)
            {

                Console.WriteLine($"Finished with error {e.Message}");
                Console.WriteLine("Press any key to close app.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}