namespace Rhea.Server;

public interface ICommand<TContext>
{
    ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}