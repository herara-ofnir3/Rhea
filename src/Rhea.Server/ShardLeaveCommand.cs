namespace Rhea.Server;

public class ShardLeaveCommand : ICommand<ShardContext>
{
	public ValueTask ExecuteAsync(ShardContext context, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}