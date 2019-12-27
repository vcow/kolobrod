using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Animator))]
	public class MonsterCrabController : MonoBehaviour
	{
		private Rigidbody2D[] _rigidBodies;

#pragma warning disable 649
		[SerializeField] private Rigidbody2D _container;
#pragma warning restore 649

		private void Start()
		{
			_rigidBodies = GetComponentsInChildren<Rigidbody2D>();
			foreach (var rigidBody in _rigidBodies)
			{
				rigidBody.simulated = false;
			}
		}

		public void OnAttack()
		{
		}
	}
}