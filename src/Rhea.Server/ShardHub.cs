using MagicOnion.Server.Hubs;
using Rhea.Shared;
using UnityEngine;

namespace Rhea.Server;

public class ShardHub(
	IContextRepository<ShardContext> contextRepository
	) : StreamingHubBase<IShardHub, IShardHubReceiver>, IShardHub
{
	ShardContext? shardContext = null;
	Player? player = null;

	protected override ValueTask OnDisconnected()
	{
		if (shardContext != null && player != null)
		{
			shardContext.CommandQueue.Enqueue(new ShardLeaveCommand
			{
				PlayerId = player.Id,
				Receiver = Client,
			});
		}
		return base.OnDisconnected();
	}

	public ValueTask JoinAsync(Guid shardId, Player player)
	{
		if (!contextRepository.TryGet(shardId, out shardContext))
		{
			throw new InvalidOperationException($"Shard not found (id: {shardId})");
		}

		shardContext.CommandQueue.Enqueue(new ShardJoinCommand
		{
			Player = player,
			Receiver = Client,
		});

		this.player = player;
		return ValueTask.CompletedTask;
	}

	public ValueTask LeaveAsync()
	{
		if (shardContext == null || player == null)
		{
			throw new InvalidOperationException($"Shard not joined");
		}

		shardContext.CommandQueue.Enqueue(new ShardLeaveCommand
		{
			PlayerId = player.Id,
			Receiver = Client,
		});

		return ValueTask.CompletedTask;
	}

	public ValueTask MoveAsync(Vector2 position, Vector2 direction)
	{
		if (shardContext == null || player == null)
		{
			throw new InvalidOperationException($"Shard not joined");
		}

		shardContext.CommandQueue.Enqueue(new ShardMoveCommand
		{
			Player = player,
			Position = position,
			Direction = direction,
		});

		return ValueTask.CompletedTask;
	}

	public ValueTask<PlayerInShard[]> GetPlayers()
	{
		if (shardContext == null || player == null)
		{
			throw new InvalidOperationException($"Shard not joined");
		}

		var players = shardContext.State.Players.Values
			.Select(p => new PlayerInShard
			{
				Player = new Player { Id = p.PlayerId, Name = p.PlayerName },
				Position = p.Position,
				Direction = p.Direction,
			})
			.ToArray();
		return ValueTask.FromResult(players);
	}
}
