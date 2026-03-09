using MagicOnion.Server.Hubs;
using Rhea.Shared;

namespace Rhea.Server;

public class ShardHub(
	IContextRepository<ShardContext> contextRepository
	) : StreamingHubBase<IShardHub, IShardHubReceiver>, IShardHub
{
	public ValueTask Join(Guid shardId, Player player) => throw new NotImplementedException();

	public ValueTask Leave() => throw new NotImplementedException();
}
