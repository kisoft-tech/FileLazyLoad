namespace MainApp.common
{
    public interface IFileStreamProvider
    {
        Stream OpenRead(string path);
    }
}