using MainApp.common;

namespace MainApp.services;

public class StreamReaderFactory : IStreamReaderFactory
{
    public StreamReader CreateStreamReader(string path)
    {
        return new StreamReader(path);
    }
}