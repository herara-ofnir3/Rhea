using MagicOnion.Server.Hubs;
using Rhea.Shared;

namespace Rhea.Server;

public class ShardHub(
	IContextRepository<ShardContext> contextRepository
	) : StreamingHubBase<IShardHub, IShardHubReceiver>, IShardHub
{
	ShardContext? shardContext = null;
	Player? player = null;

	public ValueTask Join(Guid shardId, Player player)
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

	public ValueTask Leave()
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
}
