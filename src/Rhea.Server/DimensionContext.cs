using Cysharp.Runtime.Multicast;
using Rhea.Shared;

namespace Rhea.Server;

public sealed class DimensionContext(
	Guid id,
	IMulticastSyncGroup<string, IDimensionHubReceiver> group
	) : IContext, IDisposable
{
	public Guid Id { get; } = id;

	public bool IsCompleted { get; private set; }

	public IMulticastSyncGroup<string, IDimensionHubReceiver> Group { get; } = group;

	public void Dispose()
	{
		Group.Dispose();
	}
}
