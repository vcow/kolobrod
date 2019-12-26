using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Animator))]
	public class PlayerCharacterController : MonoBehaviour
	{
#pragma warning disable 649
#pragma warning restore 649

		public void OnWalk(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;
			var value = context.ReadValue<Vector2>();
			Debug.LogFormat("Walk! {0}", value);
		}

		public void OnFire(InputAction.CallbackContext context)
		{
			
			if (context.phase != InputActionPhase.Performed) return;
			var value = context.ReadValue<Vector2>();
			Debug.LogFormat("Fire! {0}", value);
		}
	}
}