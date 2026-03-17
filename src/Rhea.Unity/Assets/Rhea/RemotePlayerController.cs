using Cysharp.Threading.Tasks;
using R3;
using Rhea.Shared;
using System;
using UnityEngine;

namespace Rhea.Unity
{
	public class RemotePlayerController : MonoBehaviour
	{
		public PlayerCharacter character;

		public IDisposable BindMoved(Observable<PlayerInShard> inShardObservable)
		{
			return inShardObservable
				.Subscribe(inShard =>
				{
					character.Move(inShard.Position, inShard.Direction);
				})
				.AddTo(this);
		}

		public void Leave()
		{
			Destroy(gameObject);
			Destroy(character.gameObject);
		}
	}
}
