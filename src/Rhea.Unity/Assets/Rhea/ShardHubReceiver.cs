#nullable enable

using R3;
using Rhea.Shared;
using System;

namespace Rhea.Unity
{
	public interface IObservableShardHubReceiver : IShardHubReceiver
	{
		Observable<PlayerInShard> JoinedAsObservable();

		Observable<Guid> LeftAsObservable();

		Observable<PlayerInShard> MovedAsObservable();
	}

	public class ShardHubReceiver : IObservableShardHubReceiver
	{
		public void OnJoined(PlayerInShard playerInShard) => Joined?.Invoke(playerInShard);

		public event Action<PlayerInShard>? Joined;

		public Observable<PlayerInShard> JoinedAsObservable() => Observable.FromEvent<PlayerInShard>(h => Joined += h, h => Joined -= h);

		public void OnLeft(Guid playerId) => Left?.Invoke(playerId);

		public event Action<Guid>? Left;

		public Observable<Guid> LeftAsObservable() => Observable.FromEvent<Guid>(h => Left += h, h => Left -= h);
				
		public void OnMoved(PlayerInShard playerInShard) => Moved?.Invoke(playerInShard);

		public event Action<PlayerInShard>? Moved;

		public Observable<PlayerInShard> MovedAsObservable() => Observable.FromEvent<PlayerInShard>(h => Moved += h, h => Moved -= h);
	}
}
