using Rhea.Shared;

namespace Rhea.Server;

public class ShardLeaveCommand : ICommand<ShardContext>
{
	public required Guid PlayerId { get; init; }

	public required IShardHubReceiver Receiver { get; init; }

	public ValueTask ExecuteAsync(ShardContext context, CancellationToken cancellationToken = default)
	{
		context.State.LeavePlayer(PlayerId);
		context.Group.Remove($"Player/${PlayerId}");
		context.Group.All.OnLeft(PlayerId);
		return ValueTask.CompletedTask;
	}
}