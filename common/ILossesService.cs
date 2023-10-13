using MainApp.models;

namespace MainApp.common
{
    public interface ILossesService
    {
        IAsyncEnumerable<EventData> ReadEventData(string csvFilePath);
        IAsyncEnumerable<Loss> CalculateLosses(string lossesPath, string contractsPath);
    }
}