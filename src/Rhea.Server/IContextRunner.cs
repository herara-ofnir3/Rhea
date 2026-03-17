namespace Rhea.Server;

public interface IContextRunner<TContext>
{
    Task RunAsync(TContext context, CancellationToken cancellationToken = default);
}