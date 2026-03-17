#nullable enable

using Cysharp.Threading.Tasks;
using MagicOnion;
using MagicOnion.Client;
using R3;
using Rhea.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Rhea.Unity
{
	public class ShardSession : IAsyncDisposable
	{
		readonly IObservableShardHubReceiver receiver = new ShardHubReceiver();
		readonly ReactiveProperty<State> state = new(new State.Standby());
		readonly CancellationTokenSource keepConnectionCts = new();
		readonly ConcurrentQueue<Func<IShardHub, UniTask>> hubCommands = new();

		public IObservableShardHubReceiver Receiver => receiver;

		public async UniTask StartAsync(CancellationToken cancellationToken = default)
		{
			if (!state.Value.IsStandby)
			{
				throw new InvalidOperationException("Shard session already started");
			}

			DebugLog("Shard session start");
			state.Value = new State.Launching();
			try
			{
				//var channel = GrpcChannelx.ForAddress(serverConfig.Origin);
				var channel = GrpcChannelx.ForAddress("http://localhost:50051");
				var service = MagicOnionClient.Create<IShardService>(channel);
				KeepConnectionAsync(channel, service, keepConnectionCts.Token).Forget();
				await WaitRunningAsync(cancellationToken);
				DebugLog("Shard session started");
			}
			catch (Exception ex)
			{
				throw new Exception("Shard session start failed, Error occurred in launch", ex);
			}
		}

		async UniTask WaitRunningAsync(CancellationToken cancellationToken)
		{
			var runningOrError = Observable.Race(
				state.OfType<State, State.Running>().Select(_ => (Exception?)null),
				state.OfType<State, State.Shutdown>().Select(s => s.Error)
				);
			var error = await runningOrError.FirstAsync(cancellationToken);
			if (error != null)
			{
				throw error;
			}
		}

		async UniTask<IShardHub> EstablishAsync(GrpcChannelx channel, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				var hub = await StreamingHubClient.ConnectAsync<IShardHub, IShardHubReceiver>(
					channel, receiver, cancellationToken: cancellationToken);
				return hub;
			}
			catch (Exception ex)
			{
				throw new Exception("Shard session start failed, Error occurred in connection established or joined.", ex);
			}
		}

		async UniTask KeepConnectionAsync(GrpcChannelx channel, IShardService service, CancellationToken shutdownToken)
		{
			var retryAttempts = 3;
			var retryIntervalSec = 1f;
			var failures = new List<Exception>();
			while (!shutdownToken.IsCancellationRequested && failures.Count < retryAttempts)
			{
				try
				{
					DebugLog("Shard server connection establish start...");
					await using var hub = await EstablishAsync(channel, shutdownToken);
					DebugLog("Shard server connection established");
					failures.Clear();
					state.Value = new State.Running(channel, service);

					var disconnectedToken = hub.WaitForDisconnectAsync()
						.AsUniTask()
						.ToCancellationToken(shutdownToken);

					while (!disconnectedToken.IsCancellationRequested)
					{
						if (hubCommands.TryDequeue(out var command))
						{
							await command(hub);
						}
						await UniTask.Yield();
					}

					if (shutdownToken.IsCancellationRequested)
					{
						break;
					}

					DebugLogWarning("Shard server disconnection detected, re-connect trying...");
					await UniTask.WaitForSeconds(retryIntervalSec);
				}
				catch (OperationCanceledException)
				{
					DebugLog("Keep connection shutdown");
				}
				catch (Exception ex)
				{
					failures.Add(ex);
					DebugLogWarning($"Shard server connection establish failed, {ex.Message}");
				}
			}
			DebugLog("Keep connection stopped");

			try
			{
				//await service.Shutdown(chatId);
				state.Value = new State.Shutdown(failures.LastOrDefault());
				DebugLog("Shard session ended");
			}
			catch (Exception ex)
			{
				DebugLogError($"Shard session end failured ({ex.Message})");
				Debug.LogException(ex);
			}
			finally
			{
				await channel.ShutdownAsync();
				await channel.DisposeAsync();
				DebugLog("Shard session finished");
			}
		}

		public UniTask EndAsync(CancellationToken cancellationToken = default)
		{
			if (state.Value is State.Shutdown)
			{
				return default;
			}

			keepConnectionCts.Cancel();
			return default;
		}

		bool disposed = false;

		public async ValueTask DisposeAsync()
		{
			if (disposed)
			{
				return;
			}

			await EndAsync();
			state.Dispose();
			keepConnectionCts.Dispose();
			disposed = true;
		}

		public async UniTask<(PlayerInShard, PlayerInShard[])> JoinAsync(Player player, CancellationToken cancellationToken)
		{
			if (state.Value is not State.Running running)
			{
				throw new Exception("Shard session is not running");
			}

			var shardId = await running.ShardService.Asign();
			var players = await running.ShardService.GetPlayers(shardId);
			hubCommands.Enqueue(async hub =>
			{
				await hub.JoinAsync(shardId, player);
			});
			var joined = await receiver
				.JoinedAsObservable()
				.FirstAsync(x => x.Player.Id == player.Id, cancellationToken);
			return (joined, players);
		}

		public async UniTask LeaveAsync(CancellationToken cancellationToken)
		{
			hubCommands.Enqueue(async hub =>
			{
				await hub.LeaveAsync();
			});

			await receiver
				.LeftAsObservable()
				.FirstAsync(cancellationToken);
		}

		public void Move(Vector2 position, Vector2 direction)
		{
			DebugLog("Requesting move command");
			hubCommands.Enqueue(async hub =>
			{
				await hub.MoveAsync(position, direction);
			});
		}

		record State()
		{
			public record Standby() : State;

			public record Launching() : State;

			public record Running(GrpcChannelx Channel, IShardService ShardService) : State;

			public record Shutdown(Exception? Error) : State;

			public bool IsStandby => this is Standby;
		}

		static void DebugLog(string message) => Debug.Log($"[{nameof(ShardSession)}] {message}");

		static void DebugLogWarning(string message) => Debug.LogWarning($"[{nameof(ShardSession)}] {message}");

		static void DebugLogError(string message) => Debug.LogError($"[{nameof(ShardSession)}] {message}");
	}
}
