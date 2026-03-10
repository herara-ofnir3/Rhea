using Rhea.Shared;
using UnityEngine;

namespace Rhea.Server;

public class ShardState
{
	readonly Dictionary<Guid, PlayerInShardState> players = [];

	public IReadOnlyDictionary<Guid, PlayerInShardState> Players => players;

	public PlayerInShardState JoinPlayer(PlayerInShard playerInShard)
	{
		if (players.ContainsKey(playerInShard.Player.Id))
		{
			throw new InvalidOperationException($"Player already exists in shard (playerId: {playerInShard.Player.Id})");
		}

		var playerState = new PlayerInShardState
		{
			PlayerId = playerInShard.Player.Id,
			PlayerName = playerInShard.Player.Name,
			Position = playerInShard.Position,
			Rotation = playerInShard.Rotation,
		};
		players.Add(playerInShard.Player.Id, playerState);
		return playerState;
	}

	public void LeavePlayer(Guid playerId)
	{
		if (!players.ContainsKey(playerId))
		{
			throw new InvalidOperationException($"Player not found in shard (playerId: {playerId})");
		}

		players.Remove(playerId);
	}

	public void MovePlayer(Guid playerId, Vector2 position, Quaternion rotation)
	{
		if (!players.ContainsKey(playerId))
		{
			throw new InvalidOperationException($"Player not found in shard (playerId: {playerId})");
		}
		var player = players[playerId];
		player.Position = position;
		player.Rotation = rotation;
	}
}
