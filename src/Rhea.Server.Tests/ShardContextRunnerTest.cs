using Cysharp.Runtime.Multicast;
using Cysharp.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rhea.Shared;
using Xunit;

namespace Rhea.Server.Tests;

public class ShardContextRunnerTests
{
    [Fact]
    public async Task RunAsync_ProcessesQueuedCommands()
    {
        // arrange
        var mockGroup = new Mock<IMulticastSyncGroup<string, IShardHubReceiver>>();
        var shardId = Guid.NewGuid();
        var context = new ShardContext(shardId, mockGroup.Object);

        var commandMock = new Mock<ICommand<ShardContext>>();
        commandMock
            .Setup(c => c.ExecuteAsync(context, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask(Task.CompletedTask))
            .Verifiable();

        context.CommandQueue.Enqueue(commandMock.Object);

        // ルーパーはデリゲートを1回だけ実行する
        using var fakeLooper = new FakeLogicLooper();
        var factory = new FakeShardLooperFactory(fakeLooper);
        var logger = NullLogger<ShardContextRunner>.Instance;
        var runner = new ShardContextRunner(factory, logger);
		var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
		cts.CancelAfter(TimeSpan.FromMilliseconds(200));

		// act
		await runner.RunAsync(context, cts.Token);

        // assert
        commandMock.Verify(c => c.ExecuteAsync(context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_StopsImmediatelyWhenContextIsCompleted()
    {
        // arrange
        var mockGroup = new Mock<IMulticastSyncGroup<string, IShardHubReceiver>>();
        var shardId = Guid.NewGuid();
		var context = new ShardContext(shardId, mockGroup.Object);
		context.Complete();

        using var fakeLooper = new FakeLogicLooper();
        var factory = new FakeShardLooperFactory(fakeLooper);
        var logger = NullLogger<ShardContextRunner>.Instance;
        var runner = new ShardContextRunner(factory, logger);
		var ct = TestContext.Current.CancellationToken;

		// act & assert: 例外なく完了すること（内部はすぐ false を返す想定）
		await runner.RunAsync(context, ct);
    }

	// 簡易フェイク ILogicLooper:
	// 渡された LogicLooperAsyncActionDelegate を内部で DynamicInvoke して実行する。
	// loopInvokeCount 回デリゲートを呼び出して完了させる（テストで制御するため）。
	class FakeLogicLooper : ILogicLooper
	{
		public FakeLogicLooper()
		{
		}

		readonly CancellationTokenSource cts = new();

		public int Id => 1;

		public int ApproximatelyRunningActions => 0;

		public TimeSpan LastProcessingDuration => TimeSpan.Zero;

		public double TargetFrameRate => 60;

		public long CurrentFrame { get; private set; } = 0;

		public async Task RegisterActionAsync(LogicLooperAsyncActionDelegate loopAction)
		{
			var started = DateTime.UtcNow;
			var prevTimestamp = 0d;
			var delay = TimeSpan.FromMicroseconds(1000 / TargetFrameRate);
			
			while (!cts.IsCancellationRequested)
			{
				var timestamp = DateTime.UtcNow.Subtract(started).TotalMilliseconds;
				var context = new LogicLooperActionContext(
					this, 
					CurrentFrame,
					(long)timestamp,
					TimeSpan.FromMilliseconds(timestamp - prevTimestamp), 
					cts.Token);
				prevTimestamp = timestamp;
				var isContinue = await loopAction(context);
				if (!isContinue)
				{
					break;
				}
				await Task.Delay(delay, cts.Token);
				CurrentFrame++;
			}
		}
		
		public void Dispose()
		{
			cts.Dispose();
		}

		// 以下は未使用だがインターフェイス実装のためダミー実装
		public Task RegisterActionAsync(LogicLooperActionDelegate loopAction, LooperActionOptions options) => Task.CompletedTask;
		public Task RegisterActionAsync(LogicLooperActionDelegate loopAction) => Task.CompletedTask;
		public Task RegisterActionAsync<TState>(LogicLooperActionWithStateDelegate<TState> loopAction, TState state) => Task.CompletedTask;
		public Task RegisterActionAsync<TState>(LogicLooperActionWithStateDelegate<TState> loopAction, TState state, LooperActionOptions options) => Task.CompletedTask;
		public Task RegisterActionAsync(LogicLooperAsyncActionDelegate loopAction, LooperActionOptions options) => RegisterActionAsync(loopAction);
		public Task RegisterActionAsync<TState>(LogicLooperAsyncActionWithStateDelegate<TState> loopAction, TState state) => Task.CompletedTask;
		public Task RegisterActionAsync<TState>(LogicLooperAsyncActionWithStateDelegate<TState> loopAction, TState state, LooperActionOptions options) => Task.CompletedTask;
		public Task ShutdownAsync(TimeSpan shutdownDelay) => Task.CompletedTask;
	}

	class FakeShardLooperFactory : IShardLooperFactory
	{
		readonly ILogicLooper _looper;
		public FakeShardLooperFactory(ILogicLooper looper) => _looper = looper;
		public ILogicLooper Create() => _looper;
	}
}