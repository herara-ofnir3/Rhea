using MagicOnion;
using System;
using System.Threading.Tasks;

namespace Rhea.Shared
{
	public interface IShardHub : IStreamingHub<IShardHub, IShardHubReceiver>
	{
		ValueTask Join(Guid shardId, Player player);

		ValueTask Leave();
	}
}
