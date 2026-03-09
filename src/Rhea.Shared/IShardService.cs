using MagicOnion;
using System;

namespace Rhea.Shared
{
	public interface IShardService : IService<IShardService>
	{
		UnaryResult<Guid> Asign();
	}
}
