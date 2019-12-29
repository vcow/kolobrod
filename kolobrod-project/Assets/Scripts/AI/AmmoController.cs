using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D))]
	public abstract class AmmoController : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField] protected float _damage;
#pragma warning restore 649

		public float Damage => _damage;

		private void Awake()
		{
			Body = GetComponent<Rigidbody2D>();
		}

		public Rigidbody2D Body { get; private set; }
	}
}