using Cysharp.Runtime.Multicast;
using Rhea.Shared;
using System.Collections.Concurrent;

namespace Rhea.Server;

public sealed class ShardContext(
	Guid id,
	IMulticastSyncGroup<string, IShardHubReceiver> group
	) : IContext, IDisposable
{
	public Guid Id { get; } = id;

	public bool IsCompleted { get; private set; }

	public IMulticastSyncGroup<string, IShardHubReceiver> Group { get; } = group;

	public ConcurrentQueue<ICommand<ShardContext>> CommandQueue { get; } = new();

	public ShardState State { get; } = new();

	public void Complete()
	{
		if (IsCompleted)
		{
			return;
		}

		IsCompleted = true;
	}

	public void Dispose()
	{
		Group.Dispose();
	}
}
