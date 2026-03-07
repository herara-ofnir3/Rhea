namespace Rhea.Server;

public interface IContextFactory<T>
{
    T Create();
}
