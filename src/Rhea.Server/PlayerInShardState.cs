using UnityEngine;

namespace Rhea.Server;

public class PlayerInShardState
{
	public Guid PlayerId { get; set; }

	public string PlayerName { get; set; } = string.Empty;

	public Vector2 Position { get; set; }

	public Quaternion Rotation { get; set; }
}
