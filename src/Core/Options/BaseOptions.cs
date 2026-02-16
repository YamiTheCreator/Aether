namespace Aether.Core.Options;

public class BaseOptions
{
    public int MaxEntities { get; init; } = 500_000;
    public int InitialCapacity { get; init; } = 1024;
}