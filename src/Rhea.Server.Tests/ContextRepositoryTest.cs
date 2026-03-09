using Moq;
using Xunit;

namespace Rhea.Server.Tests;

public sealed class ContextRepositoryTest
{
	/// <summary>
	/// コンテキストの生成と実行、削除を確認します
	/// </summary>
	/// <returns></returns>
    [Fact]
    public async Task CreateAndRunToRemove()
    {
        var contextId1 = Guid.NewGuid();
        var context1 = new MockContext { Id = contextId1 };

        var longRunningContextId = Guid.NewGuid();
        var longRunningContext = new MockContext { Id = longRunningContextId };
        CancellationToken? longRunningContextCancelToken = null;

        var runner = new Mock<IContextRunner<MockContext>>();
        runner
            .Setup(m => m.RunAsync(context1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        runner
            .Setup(m => m.RunAsync(longRunningContext, It.IsAny<CancellationToken>()))
            .Returns<MockContext, CancellationToken>(async (c, ct) =>
            {
                longRunningContextCancelToken = ct;
                await Task.Delay(1000, ct);
				Assert.Fail("Task not canceled");
            });

        var repository = new ContextRepository<MockContext>(runner.Object);

        var created1 = repository.CreateAndRun(() => context1);
        Assert.Equal(contextId1, created1.Id);

        var everRunningContextCreated = repository.CreateAndRun(() => longRunningContext);
        Assert.Equal(longRunningContextId, longRunningContext.Id);

        Assert.True(repository.TryGet(contextId1, out var got1));
        Assert.Equal(context1, got1);

        Assert.True(repository.TryGet(longRunningContextId, out var got2));
        Assert.Equal(longRunningContext, got2);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        repository.Remove(contextId1);
        Assert.False(repository.TryGet(contextId1, out var _));

        repository.Remove(longRunningContextId);
        Assert.False(repository.TryGet(longRunningContextId, out var _));
        Assert.True(longRunningContextCancelToken.HasValue);
        Assert.True(longRunningContextCancelToken.Value.IsCancellationRequested); // 実行中に削除された場合はタスクをキャンセルします
    }

	/// <summary>
	/// コンテキストの生成と実行、削除を確認します。
	/// 対象のコンテキストが Disposable の場合、正常に Dispose される必要があります。
	/// </summary>
	/// <returns></returns>
	[Fact]
    public async Task CreateAndRunToRemove_DisposableContextThenDisposed()
    {
        var contextId1 = Guid.NewGuid();
        var context1 = new DisposableContext { Id = contextId1 };

        var runner = new Mock<IContextRunner<DisposableContext>>();
        runner
            .Setup(m => m.RunAsync(context1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var repository = new ContextRepository<DisposableContext>(runner.Object);

        var created1 = repository.CreateAndRun(() => context1);
        Assert.Equal(contextId1, created1.Id);

        Assert.True(repository.TryGet(contextId1, out var got1));
        Assert.Equal(context1, got1);

        await Task.Delay(50, TestContext.Current.CancellationToken);
        repository.Remove(contextId1);
        Assert.False(repository.TryGet(contextId1, out var _));
        Assert.True(context1.Disposed);
    }

	/// <summary>
	/// 存在しないタスクの削除が失敗することを確認します。
	/// </summary>
    [Fact]
    public void Remove_NotFoundThenFails()
    {
        var contextId1 = Guid.NewGuid();
        var context1 = new DisposableContext { Id = contextId1 };

        var runner = new Mock<IContextRunner<DisposableContext>>();
        runner
            .Setup(m => m.RunAsync(context1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var repository = new ContextRepository<DisposableContext>(runner.Object);

        // コンテキストが 0件 の場合、必ず失敗します
        Assert.Throws<InvalidOperationException>(() => repository.Remove(Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(() => repository.Remove(contextId1));

        // コンテキストが 1件 の場合、存在しないIDは失敗します
        repository.CreateAndRun(() => context1);
        Assert.Throws<InvalidOperationException>(() => repository.Remove(Guid.NewGuid()));

        // 存在するIDは成功します
        repository.Remove(contextId1);

        // 削除されたIDは失敗します
        Assert.Throws<InvalidOperationException>(() => repository.Remove(contextId1));
    }

	public class MockContext : IContext
    {
        public Guid Id { get; set; }
    }

    public class DisposableContext : IContext, IDisposable
    {
        public Guid Id { get; set; }

        public bool Disposed { get; set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
