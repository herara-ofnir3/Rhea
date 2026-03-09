using Cysharp.Runtime.Multicast;
using MagicOnion;
using MagicOnion.Server;
using Rhea.Shared;

namespace Rhea.Server;

public class DimensionService(
	IContextRepository<DimensionContext> contextRepository,
	IMulticastGroupProvider groupProvider,
	ILogger<DimensionService> logger
	) : ServiceBase<IDimensionService>, IDimensionService
{
	static readonly Guid alphaId = new("1DEB75D0-F03E-4EB6-B431-BD523434DFFC");

	public UnaryResult<Guid> Asign()
	{
		if (contextRepository.TryGet(alphaId, out var alpha))
		{
			logger.LogInformation("Aplha dimension already exists");
			return UnaryResult.FromResult(alpha.Id);
		}

		alpha = contextRepository.CreateAndRun(() =>
		{
			var group = groupProvider.GetOrAddSynchronousGroup<string, IDimensionHubReceiver>($"Dimension/{alphaId}");
			return new DimensionContext(alphaId, group);
		});
		logger.LogInformation("Aplha dimension started");
		return UnaryResult.FromResult(alpha.Id);
	}
}
