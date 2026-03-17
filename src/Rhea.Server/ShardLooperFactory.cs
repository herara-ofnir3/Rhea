using Cysharp.Threading;

namespace Rhea.Server;

public interface IShardLooperFactory
{
	ILogicLooper Create();
}

public class ShardLooperFactory : IShardLooperFactory
{
	public ILogicLooper Create() => new LogicLooper(30);
}