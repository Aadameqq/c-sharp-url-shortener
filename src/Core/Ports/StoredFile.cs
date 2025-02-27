namespace Core.Ports;

public interface StoredFile
{
    public Task CopyToStream(Stream stream);
    public string GetFileName();
    public long GetLength();
    public string GetContentType();
}
