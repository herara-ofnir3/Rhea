using MessagePack;
using System;

namespace Rhea.Shared
{
	[MessagePackObject]
	public class Dimension
	{
		[Key(0)]
		public Guid Id { get; set; }
	}
}
