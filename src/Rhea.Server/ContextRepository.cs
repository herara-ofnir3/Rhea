using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Rhea.Server;

public interface IContextRepository<TContext> 
    where TContext : class, IContext
{
    TContext CreateAndRun();

    bool TryGet(Guid id, [NotNullWhen(true)] out TContext? context);

    void Remove(Guid id);
}

public class ContextRepository<TContext>(IContextFactory<TContext> factory, IContextRunner<TContext> loopRunner) : IContextRepository<TContext>
    where TContext : class, IContext
{
    private readonly ConcurrentDictionary<Guid, ContextWork<TContext>> _contexts = new();

    public TContext CreateAndRun()
    {
        var context = factory.Create();
        var cts = new CancellationTokenSource();
        var loopTask = loopRunner.RunAsync(context, cts.Token);
        _contexts[context.Id] = new ContextWork<TContext>(context, loopTask, cts);
        return context;
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
            throw new InvalidOperationException($"Context {id} is not found");
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
