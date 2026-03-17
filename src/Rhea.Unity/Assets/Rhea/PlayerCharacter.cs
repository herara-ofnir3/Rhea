using System;
using UnityEngine;

namespace Rhea.Unity
{
	public class PlayerCharacter : MonoBehaviour
	{
		IsometricCharacterRenderer isoRenderer;
		Rigidbody2D rbody;

		public event Action<PlayerMoved> Moved;

		private void Awake()
		{
			rbody = GetComponent<Rigidbody2D>();
			isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
		}

		public Vector2 Position => rbody.position;

		PlayerMoved lastMoved;

		public void Move(Vector2 position, Vector2 direction)
		{
			var moved = new PlayerMoved(position, direction);
			if (moved == lastMoved)
			{
				return;
			}
			isoRenderer.SetDirection(direction);
			rbody.MovePosition(position);
			OnMoved(moved);
			lastMoved = moved;
		}

		public void OnMoved(PlayerMoved moved) => Moved?.Invoke(moved);
	}

	public record PlayerMoved(Vector2 Position, Vector2 Direction);
}
