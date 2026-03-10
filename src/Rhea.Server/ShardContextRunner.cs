namespace Rhea.Server;

public class ShardContextRunner(
	IShardLooperFactory looperFactory,
	ILogger<ShardContextRunner> logger
	) : IContextRunner<ShardContext>
{
	public async Task RunAsync(ShardContext context, CancellationToken cancellationToken)
	{
		var looper = looperFactory.Create();
		logger.LogInformation("Shard started {context.Id}", context.Id);

		await looper.RegisterActionAsync(async loopContext =>
		{
			if (context.IsCompleted || cancellationToken.IsCancellationRequested)
			{
				return false;
			}

			try
			{
				while (context.CommandQueue.TryDequeue(out var command))
				{
					var commandType = command.GetType();
					logger.LogInformation("Command executing {commandType}", commandType);
					await command.ExecuteAsync(context, cancellationToken);
				}
			}
			catch (OperationCanceledException)
			{
				logger.LogInformation("Shard canceled {context.Id}", context.Id);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Shard error {context.Id}", context.Id);
			}

			return true;
		});

		logger.LogInformation("Shard ended {context.Id}", context.Id);
	}
}
