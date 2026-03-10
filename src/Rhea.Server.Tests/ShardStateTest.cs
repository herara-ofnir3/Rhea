using Rhea.Shared;
using UnityEngine;
using Xunit;

namespace Rhea.Server.Tests;

public sealed class ShardStateTest
{
	[Fact]
	public void JoinAndLeavePlayer()
	{
		var shardState = new ShardState();
		var playerId = Guid.NewGuid();
		var player = new Player
		{
			Id = playerId,
			Name = "TestPlayer",
		};
		var playerInShard = new PlayerInShard
		{
			Player = player,
			Position = new Vector2(1, 2),
			Rotation = new Quaternion(1, 2, 3, 4)
		};
		var actual = shardState.JoinPlayer(playerInShard);
		Assert.Equal("TestPlayer", actual.PlayerName);
		Assert.Equal(new Vector2(1, 2), actual.Position);
		AssertQuaternionEqual(new Quaternion(1, 2, 3, 4), actual.Rotation);

		Assert.True(shardState.Players.ContainsKey(playerId));
		var playerState = shardState.Players[playerId];
		Assert.Equal("TestPlayer", playerState.PlayerName);
		Assert.Equal(new Vector2(1, 2), playerState.Position);
		AssertQuaternionEqual(new Quaternion(1, 2, 3, 4), playerState.Rotation);

		shardState.LeavePlayer(playerId);
		Assert.False(shardState.Players.ContainsKey(playerId));
	}

	[Fact]
	public void JoinPlayer_WhenAlreadyJoined_ShouldThrow()
	{
		var shardState = new ShardState();
		var playerId = Guid.NewGuid();
		var playerInShard = new PlayerInShard
		{
			Player = new Player
			{
				Id = playerId,
				Name = "TestPlayer",
			},
			Position = Vector2.zero,
			Rotation = Quaternion.identity
		};
		shardState.JoinPlayer(playerInShard);
		Assert.Throws<InvalidOperationException>(() => shardState.JoinPlayer(playerInShard));
	}

	[Fact]
	public void LeavePlayer_WhenNotJoined_ShouldThrow()
	{
		var shardState = new ShardState();
		var playerId = Guid.NewGuid();
		Assert.Throws<InvalidOperationException>(() => shardState.LeavePlayer(playerId));
	}

	[Fact]
	public void MovePlayer_WhenJoined_ShouldUpdatePositionAndRotation()
	{
		var shardState = new ShardState();
		var playerId = Guid.NewGuid();
		var playerInShard = new PlayerInShard
		{
			Player = new Player
			{
				Id = playerId,
				Name = "TestPlayer",
			},
			Position = Vector2.zero,
			Rotation = Quaternion.identity
		};
		shardState.JoinPlayer(playerInShard);
		var newPosition = new Vector2(3, 4);
		var newRotation = new Quaternion(5, 6, 7, 8);
		shardState.MovePlayer(playerId, newPosition, newRotation);
		var playerState = shardState.Players[playerId];
		Assert.Equal(newPosition, playerState.Position);
		AssertQuaternionEqual(newRotation, playerState.Rotation);
	}

	[Fact]
	public void MovePlayer_WhenNotJoined_ShouldThrow()
	{
		var shardState = new ShardState();
		var playerId = Guid.NewGuid();
		var newPosition = new Vector2(3, 4);
		var newRotation = new Quaternion(5, 6, 7, 8);
		Assert.Throws<InvalidOperationException>(() => shardState.MovePlayer(playerId, newPosition, newRotation));
	}

	static void AssertQuaternionEqual(Quaternion expected, Quaternion actual)
	{
		Assert.Equal(expected.x, actual.x);
		Assert.Equal(expected.y, actual.y);
		Assert.Equal(expected.z, actual.z);
		Assert.Equal(expected.w, actual.w);
	}
}
