using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent]
	public abstract class WeaponController : MonoBehaviour
	{
		[SerializeField] protected float _damage;
		[SerializeField] protected float _rechargeTime;
		[SerializeField] protected GameObject _ammoPrefab;

		public abstract bool Shut(Vector2 targetPoint);
	}
}