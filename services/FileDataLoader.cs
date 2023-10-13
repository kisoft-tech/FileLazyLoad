using MainApp.common;
using System.Text;

namespace MainApp.services
{
    public class FileDataLoader : IDataLoader
    {
        private readonly string filePath;
        private readonly IFileStreamProvider fileStreamProvider;

        public FileDataLoader(string filePath, IFileWrapper fileWrapper, IFileStreamProvider fileStreamProvider)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }
            if (!fileWrapper.Exists(filePath))
            {
                throw new FileNotFoundException("File does not exist", nameof(filePath));
            }
            this.filePath = filePath;
            this.fileStreamProvider = fileStreamProvider;
        }

        public async IAsyncEnumerable<string> LoadData()
        {
            await using var stream = fileStreamProvider.OpenRead(filePath);
            await using var buffer = new BufferedStream(stream);
            using var reader = new StreamReader(buffer, Encoding.UTF8, false, 4096, true);

            while (await reader.ReadLineAsync() is { } line)
            {
                CurrentLineNumber++;
                yield return line;
            }
        }

        public int CurrentLineNumber { get; private set; } = 0;
    }
}