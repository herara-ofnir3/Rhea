using Cysharp.Runtime.Multicast;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rhea.Shared;
using Xunit;

namespace Rhea.Server.Tests;

public class ShardServiceTest
{
    static readonly Guid AlphaId = new("1DEB75D0-F03E-4EB6-B431-BD523434DFFC");

    [Fact]
    public async Task Asign_WhenAlphaAlreadyExists_ShouldNotCallCreateAndRun()
    {
		// arrange
		var mockGroupProvider = new Mock<IMulticastGroupProvider>();
		var mockGroup = new Mock<IMulticastSyncGroup<string, IShardHubReceiver>>();
		var existingId = Guid.NewGuid(); // 検証用のため AlphaId 以外の ID を使用します
		var existingContext = new ShardContext(existingId, mockGroup.Object);
        var mockRepo = new Mock<IContextRepository<ShardContext>>();
		mockRepo
			.Setup(m => m.TryGet(AlphaId, out existingContext))
			.Returns(true)
			.Verifiable();
		
        var logger = NullLogger<ShardService>.Instance;
        var service = new ShardService(mockRepo.Object, mockGroupProvider.Object, logger);

        // act
        var result = await service.Asign();

        // assert: TryGet が呼ばれ、CreateAndRun は呼ばれない
		mockRepo.Verify();
		mockRepo.Verify(m => m.CreateAndRun(It.IsAny<Func<ShardContext>>()), Times.Never);
		Assert.Equal(existingId, result);
	}

	[Fact]
	public async Task Asign_WhenAlphaMissing_ShouldCallCreateAndRun()
	{
		// arrange
		var mockGroupProvider = new Mock<IMulticastGroupProvider>();
		var mockGroup = new Mock<IMulticastSyncGroup<string, IShardHubReceiver>>();
		mockGroupProvider
			.Setup(m => m.GetOrAddSynchronousGroup<string, IShardHubReceiver>($"Shard/{AlphaId}"))
			.Returns(mockGroup.Object)
			.Verifiable();
		var creatingContext = new ShardContext(AlphaId, mockGroup.Object);
		ShardContext? existingContext = null;
		var mockRepo = new Mock<IContextRepository<ShardContext>>();
		mockRepo
			.Setup(m => m.TryGet(AlphaId, out existingContext))
			.Returns(false)
			.Verifiable();
		mockRepo
			.Setup(m => m.CreateAndRun(It.IsAny<Func<ShardContext>>()))
			.Returns(creatingContext)
			.Callback((Func<ShardContext> create) => create());

		var logger = NullLogger<ShardService>.Instance;
		var service = new ShardService(mockRepo.Object, mockGroupProvider.Object, logger);

		// act
		var result = await service.Asign();

		// assert: TryGet が呼ばれ、CreateAndRun が呼ばれる
		mockGroupProvider.Verify();
		mockRepo.Verify();
		Assert.Equal(AlphaId, result);
	}
}