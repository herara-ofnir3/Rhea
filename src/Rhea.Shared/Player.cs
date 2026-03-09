using MagicOnion;
using MessagePack;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Rhea.Shared
{
	[MessagePackObject]
	public class Player
	{
		[Key(0)]
		public Guid Id { get; set; }

		[Key(1)]
		public string Name { get; set; } = string.Empty;
	}
}
