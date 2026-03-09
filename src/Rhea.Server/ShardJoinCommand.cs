namespace Rhea.Server;

public class ShardJoinCommand : ICommand<ShardContext>
{
	public ValueTask ExecuteAsync(ShardContext context, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
