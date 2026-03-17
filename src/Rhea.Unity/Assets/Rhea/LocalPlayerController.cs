using R3;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rhea.Unity
{
	public class LocalPlayerController : MonoBehaviour
	{
		public PlayerCharacter character;
		public InputActionProperty moveActionProperty;
		public float movementSpeed = 1f;

		void FixedUpdate()
		{
			//float horizontalInput = Input.GetAxis("Horizontal");
			//float verticalInput = Input.GetAxis("Vertical");
			//Vector2 inputVector = new Vector2(horizontalInput, verticalInput);

			if (character == null)
			{
				return;
			}
			Vector2 currentPos = character.Position;
			var inputVector = moveActionProperty.action.ReadValue<Vector2>();
			inputVector = Vector2.ClampMagnitude(inputVector, 1);
			Vector2 movement = inputVector * movementSpeed;
			Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
			character.Move(newPos, movement);
		}

		public IDisposable SubscribeMoved(Action<PlayerMoved> onMoved)
		{
			character.Moved += onMoved;
			return Disposable.Create(() => character.Moved -= onMoved);
		}
	}
}
