using System;

namespace Rhea.Shared
{
	public interface IShardHubReceiver
	{
		void OnJoined(PlayerInShard playerInShard);

		void OnLeft(Guid playerId);
	}
}
