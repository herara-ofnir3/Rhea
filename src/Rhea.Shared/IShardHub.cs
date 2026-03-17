using MagicOnion;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Rhea.Shared
{
	public interface IShardHub : IStreamingHub<IShardHub, IShardHubReceiver>
	{
		ValueTask JoinAsync(Guid shardId, Player player);

		ValueTask LeaveAsync();

		ValueTask MoveAsync(Vector2 position, Vector2 direction);

		ValueTask<PlayerInShard[]> GetPlayers();
	}
}
