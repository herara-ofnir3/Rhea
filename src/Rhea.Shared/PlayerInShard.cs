using MessagePack;
using UnityEngine;

namespace Rhea.Shared
{
	[MessagePackObject]
	public class PlayerInShard
	{
		[Key(0)]
		public Player Player { get; set; } = new Player();

		[Key(1)]
		public Vector2 Position { get; set; } = Vector2.zero;

		[Key(2)]
		public Vector2 Direction { get; set; } = Vector2.down;
	}
}
