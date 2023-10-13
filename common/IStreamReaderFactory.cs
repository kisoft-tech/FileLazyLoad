namespace MainApp.common;

public interface IStreamReaderFactory
{
    StreamReader CreateStreamReader(string path);
}