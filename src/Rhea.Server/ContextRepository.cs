using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Rhea.Server;

public interface IContextRepository<TContext> 
    where TContext : class, IContext
{
	TContext CreateAndRun(Func<TContext> create);

    bool TryGet(Guid id, [NotNullWhen(true)] out TContext? context);

    void Remove(Guid id);
}

public class ContextRepository<TContext>(
	IContextRunner<TContext> runner
	) : IContextRepository<TContext>
    where TContext : class, IContext
{
	readonly ConcurrentDictionary<Guid, ContextWork<TContext>> _contexts = new();

	public TContext CreateAndRun(Func<TContext> create)
	{
		var context = create();
		Run(context);
		return context;
	}

	void Run(TContext context)
	{
		var cts = new CancellationTokenSource();
		var loopTask = runner.RunAsync(context, cts.Token);
		_contexts[context.Id] = new ContextWork<TContext>(context, loopTask, cts);
	}

	public bool TryGet(Guid id, [NotNullWhen(true)] out TContext? context)
	{
		if (_contexts.TryGetValue(id, out var contextAndLoopTask))
		{
			context = contextAndLoopTask.Context;
			return true;
		}

		context = null;
		return false;
	}

	public void Remove(Guid id)
	{
		if (!_contexts.Remove(id, out var removed))
		{
			throw new InvalidOperationException($"Context not found (id: {id})");
		}

		if (!removed.LoopTask.IsCompleted)
		{
			removed.Cts.Cancel();
		}

		if (removed.Context is IDisposable disposable)
		{
			disposable.Dispose();
		}

		removed.Cts.Dispose();
	}
}

public record struct ContextWork<TContext>(
    TContext Context, 
    Task LoopTask, 
    CancellationTokenSource Cts
    ) where TContext : class, IContext;
