using Rhea.Shared;
using UnityEngine;

namespace Rhea.Server;

public class ShardJoinCommand : ICommand<ShardContext>
{
	public required Player Player { get; init; }

	public required IShardHubReceiver Receiver { get; init; }

	public ValueTask ExecuteAsync(ShardContext context, CancellationToken cancellationToken = default)
	{
		var playerInShard = new PlayerInShard
		{
			Player = Player,
			Position = Vector2.zero,
			Rotation = Quaternion.identity,
		};
		context.State.JoinPlayer(playerInShard);
		context.Group.Add($"Player/${Player.Id}", Receiver);
		context.Group.All.OnJoined(playerInShard);
		return ValueTask.CompletedTask;
	}
}
