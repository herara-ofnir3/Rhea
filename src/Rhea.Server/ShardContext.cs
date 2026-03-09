using Cysharp.Runtime.Multicast;
using Rhea.Shared;

namespace Rhea.Server;

public sealed class ShardContext(
	Guid id,
	IMulticastSyncGroup<string, IShardHubReceiver> group
	) : IContext, IDisposable
{
	public Guid Id { get; } = id;

	public bool IsCompleted { get; private set; }

	public IMulticastSyncGroup<string, IShardHubReceiver> Group { get; } = group;

	public void Dispose()
	{
		Group.Dispose();
	}
}
