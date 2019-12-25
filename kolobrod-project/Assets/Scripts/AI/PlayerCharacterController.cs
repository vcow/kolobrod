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
			Debug.Log("Walk!");
		}

		public void OnFire(InputAction.CallbackContext context)
		{
			Debug.Log("Fire!");
		}
	}
}