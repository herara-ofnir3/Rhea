using Cysharp.Threading.Tasks;
using Rhea.Shared;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;
using R3;

namespace Rhea.Unity
{
	public class MainPresenter : IAsyncStartable
	{
		public readonly IMainTransitions mainTransitions;

		public MainPresenter(IMainTransitions mainTransitions)
		{
			this.mainTransitions = mainTransitions;
		}

		public async UniTask StartAsync(CancellationToken cancellation = default)
		{
			Debug.Log($"[{nameof(MainPresenter)}] Started");
			var mainWorld = await mainTransitions.StartedAsync(cancellation);
			var localPlayer = new Player
			{
				Id = Guid.NewGuid(),
				Name = $"Tester",
			};
			Debug.Log($"[{nameof(MainPresenter)}] Local player: {localPlayer.Id}");
			await using var session = new ShardSession();
			await session.StartAsync(cancellation);

			var (local, remotes) = await session.JoinAsync(localPlayer, cancellation);
			var localPlayerController = mainWorld.SpawnLocalPlayer(local);
			using var _ = localPlayerController.SubscribeMoved(x => session.Move(x.Position, x.Direction));

			foreach (var remote in remotes)
			{
				RemotePlayerLoopAsync(remote, session, mainWorld, cancellation).Forget();
			}

			try
			{
				while (!cancellation.IsCancellationRequested)
				{
					var joined = await session.Receiver
						.JoinedAsObservable()
						.FirstAsync(x => x.Player.Id != localPlayer.Id, cancellation);

					RemotePlayerLoopAsync(joined, session, mainWorld, cancellation).Forget();
					await UniTask.Yield(cancellation);
				}
			}
			catch (OperationCanceledException)
			{
				Debug.Log($"[{nameof(MainPresenter)}] Canceled");
			}
			finally
			{
				//await session.LeaveAsync(cancellation);
				Debug.Log($"[{nameof(MainPresenter)}] Finished");
			}
		}

		public async UniTask RemotePlayerLoopAsync(
			PlayerInShard remote, 
			ShardSession session, 
			IMainWorldView mainWorld,
			CancellationToken cancellation)
		{
			Debug.Log($"[{nameof(MainPresenter)}] Remote player joined: {remote.Player.Id}");
			var controller = mainWorld.SpawnRemotePlayer(remote);
			var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
			var movedObservable = session.Receiver
				.MovedAsObservable()
				.Where(x => x.Player.Id == remote.Player.Id);
			using var _ = controller.BindMoved(movedObservable);

			await session.Receiver
				.LeftAsObservable()
				.FirstAsync(id => id == remote.Player.Id, cancellation);

			controller.Leave();
			Debug.Log($"[{nameof(MainPresenter)}] Remote player left: {remote.Player.Id}");
		}
	}
}
