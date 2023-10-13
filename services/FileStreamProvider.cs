using MainApp.common;

namespace MainApp.services
{
    public class FileStreamProvider : IFileStreamProvider
    {
        public Stream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        }
    }
}


