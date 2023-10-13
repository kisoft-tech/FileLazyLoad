//using MainApp.common;
//using MainApp.models;
//using System.Text;

namespace MainApp.common
{
    public interface IDataLoader
    {
        IAsyncEnumerable<string> LoadData();
        int CurrentLineNumber { get; }
    }

}