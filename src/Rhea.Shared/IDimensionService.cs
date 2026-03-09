using MagicOnion;
using System;

namespace Rhea.Shared
{
	public interface IDimensionService : IService<IDimensionService>
	{
		UnaryResult<Guid> Asign();
	}
}
