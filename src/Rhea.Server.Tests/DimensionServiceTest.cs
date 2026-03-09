using Cysharp.Runtime.Multicast;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rhea.Shared;
using Xunit;

namespace Rhea.Server.Tests;

public class DimensionServiceTests
{
    static readonly Guid AlphaId = new Guid("1DEB75D0-F03E-4EB6-B431-BD523434DFFC");

    [Fact]
    public async Task Asign_WhenAlphaAlreadyExists_ShouldNotCallCreateAndRun()
    {
		// arrange
		var mockGroupProvider = new Mock<IMulticastGroupProvider>();
		var mockGroup = new Mock<IMulticastSyncGroup<string, IDimensionHubReceiver>>();
		var existingId = Guid.NewGuid(); // 検証用のため AlphaId 以外の ID を使用します
		var existingContext = new DimensionContext(existingId, mockGroup.Object);
        var mockRepo = new Mock<IContextRepository<DimensionContext>>();
		mockRepo
			.Setup(m => m.TryGet(AlphaId, out existingContext))
			.Returns(true)
			.Verifiable();
		
        var logger = NullLogger<DimensionService>.Instance;
        var service = new DimensionService(mockRepo.Object, mockGroupProvider.Object, logger);

        // act
        var result = await service.Asign();

        // assert: TryGet が呼ばれ、CreateAndRun は呼ばれない
		mockRepo.Verify();
		mockRepo.Verify(m => m.CreateAndRun(It.IsAny<Func<DimensionContext>>()), Times.Never);
		Assert.Equal(existingId, result);
	}

	[Fact]
	public async Task Asign_WhenAlphaMissing_ShouldCallCreateAndRun()
	{
		// arrange
		var mockGroupProvider = new Mock<IMulticastGroupProvider>();
		var mockGroup = new Mock<IMulticastSyncGroup<string, IDimensionHubReceiver>>();
		mockGroupProvider
			.Setup(m => m.GetOrAddSynchronousGroup<string, IDimensionHubReceiver>($"Dimension/{AlphaId}"))
			.Returns(mockGroup.Object)
			.Verifiable();
		var creatingContext = new DimensionContext(AlphaId, mockGroup.Object);
		DimensionContext? existingContext = null;
		var mockRepo = new Mock<IContextRepository<DimensionContext>>();
		mockRepo
			.Setup(m => m.TryGet(AlphaId, out existingContext))
			.Returns(false)
			.Verifiable();
		mockRepo
			.Setup(m => m.CreateAndRun(It.IsAny<Func<DimensionContext>>()))
			.Returns(creatingContext)
			.Callback((Func<DimensionContext> create) => create());

		var logger = NullLogger<DimensionService>.Instance;
		var service = new DimensionService(mockRepo.Object, mockGroupProvider.Object, logger);

		// act
		var result = await service.Asign();

		// assert: TryGet が呼ばれ、CreateAndRun が呼ばれる
		mockGroupProvider.Verify();
		mockRepo.Verify();
		Assert.Equal(AlphaId, result);
	}
}