namespace ii.RobertHat.Model;

public enum CompressionType
{
    None = 0,
    Zlib = 1,
    Bzip2 = 2,
}

public abstract class Resource
{
    public int ResourceId { get; set; }
}
