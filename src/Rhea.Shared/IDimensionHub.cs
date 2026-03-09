using MagicOnion;
using System;
using System.Threading.Tasks;

namespace Rhea.Shared
{
	public interface IDimensionHub : IStreamingHub<IDimensionHub, IDimensionHubReceiver>
	{
		ValueTask Join(Guid dimensionId, Player player);

		ValueTask Leave();
	}
}
