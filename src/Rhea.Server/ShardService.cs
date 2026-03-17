using Cysharp.Runtime.Multicast;
using MagicOnion;
using MagicOnion.Server;
using Rhea.Shared;
using System.Numerics;

namespace Rhea.Server;

public class ShardService(
	IContextRepository<ShardContext> contextRepository,
	IMulticastGroupProvider groupProvider,
	ILogger<ShardService> logger
	) : ServiceBase<IShardService>, IShardService
{
	static readonly Guid alphaId = new("1DEB75D0-F03E-4EB6-B431-BD523434DFFC");

	public UnaryResult<Guid> Asign()
	{
		if (contextRepository.TryGet(alphaId, out var alpha))
		{
			logger.LogInformation("Aplha shard already exists");
			return UnaryResult.FromResult(alpha.Id);
		}

		alpha = contextRepository.CreateAndRun(() =>
		{
			var group = groupProvider.GetOrAddSynchronousGroup<string, IShardHubReceiver>($"Shard/{alphaId}");
			return new ShardContext(alphaId, group);
		});
		logger.LogInformation("Aplha shard started");
		return UnaryResult.FromResult(alpha.Id);
	}

	public UnaryResult<PlayerInShard[]> GetPlayers(Guid shardId)
	{
		if (!contextRepository.TryGet(shardId, out var shardContext))
		{
			logger.LogInformation("Shard not exists {shardId}", shardId);
			return UnaryResult.FromResult(Array.Empty<PlayerInShard>());
		}

		var players = shardContext.State.Players.Values
			.Select(p => new PlayerInShard
			{
				Player = new Player { Id = p.PlayerId, Name = p.PlayerName },
				Position = p.Position,
				Direction = p.Direction,
			})
			.ToArray();
		return UnaryResult.FromResult(players);
	}
}
