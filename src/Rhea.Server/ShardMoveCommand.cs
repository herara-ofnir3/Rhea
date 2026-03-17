using Rhea.Shared;
using UnityEngine;

namespace Rhea.Server;

public class ShardMoveCommand : ICommand<ShardContext>
{
	public required Player Player { get; init; }

	public required Vector2 Position { get; init; }

	public required Vector2 Direction { get; init; }

	public ValueTask ExecuteAsync(ShardContext context, CancellationToken cancellationToken = default)
	{
		context.State.MovePlayer(Player.Id, Position, Direction);
		context.Group.All.OnMoved(new PlayerInShard
		{
			Player = Player,
			Position = Position,
			Direction = Direction,
		});
		return ValueTask.CompletedTask;
	}
}
