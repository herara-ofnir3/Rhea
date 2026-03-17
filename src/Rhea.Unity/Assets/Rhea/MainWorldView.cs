using Rhea.Shared;
using UnityEngine;

namespace Rhea.Unity
{
	public interface IMainWorldView
	{
		LocalPlayerController SpawnLocalPlayer(PlayerInShard playerInShard);

		RemotePlayerController SpawnRemotePlayer(PlayerInShard playerInShard);
	}

	public class MainWorldView : MonoBehaviour, IMainWorldView
	{
		public CameraFollow cameraFollow;
		public PlayerCharacter playerPrefab;
		public LocalPlayerController localPlayerControllerPrefab;
		public RemotePlayerController remotePlayerControllerPrefab;

		public LocalPlayerController SpawnLocalPlayer(PlayerInShard playerInShard)
		{
			var controller = Instantiate(localPlayerControllerPrefab);
			var character = Instantiate(playerPrefab);
			controller.character = character;
			cameraFollow.followTarget = character.transform;
			return controller;
		}

		public RemotePlayerController SpawnRemotePlayer(PlayerInShard playerInShard)
		{
			var controller = Instantiate(remotePlayerControllerPrefab);
			var character = Instantiate(playerPrefab);
			controller.character = character;
			character.Move(playerInShard.Position, playerInShard.Direction);
			return controller;
		}
	}
}
