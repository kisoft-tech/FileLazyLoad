using MainApp.models;

namespace MainApp.common
{
    public interface IDealLoader
    {
        IAsyncEnumerable<Deal> LoadDeals();
    }
}