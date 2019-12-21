using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent]
	public abstract class WeaponController : MonoBehaviour
	{
		private float timestamp;

		[SerializeField] protected float _rechargeTime;
		[SerializeField] protected GameObject _ammoPrefab;
		[SerializeField] protected Transform _spawnPoint;
		[SerializeField] protected SpriteRenderer _view;

		public bool Shut(Vector2 targetPoint)
		{
			if (!WeaponIsReady) return false;
			timestamp = Time.time + _rechargeTime;
			return DoShut(targetPoint);
		}

		public bool WeaponIsReady => Time.time > timestamp;

		public Color Color
		{
			set => _view.color = value;
		}

		protected abstract bool DoShut(Vector2 targetPoint);
	}
}