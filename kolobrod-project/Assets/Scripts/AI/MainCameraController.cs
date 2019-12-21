using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
	public class MainCameraController : MonoBehaviour
	{
		private Transform _player;
		private Transform _transform;
		private Vector3 _initialPosition;

		private Vector3 _lastPlayerPosition;
		private float _velocity;

#pragma warning disable 649
		[SerializeField] private float _acceleration = 10f;
#pragma warning restore 649

		private void Start()
		{
			_transform = transform;
			_initialPosition = _transform.position;
		}

		private void Update()
		{
			if (_player)
			{
				var acc = 0f;
				var pp = _player.position;
				if (pp.x > _lastPlayerPosition.x)
				{
					acc = _acceleration;
				}
				else if (pp.x < _lastPlayerPosition.x)
				{
					acc = -_acceleration;
				}

				_lastPlayerPosition = pp;

				var p = new Vector3(_player.position.x + acc, _initialPosition.y, _initialPosition.z);
				_transform.position = Vector3.Lerp(_transform.position, p, Time.deltaTime);
			}
			else
			{
				_player = GameObject.FindGameObjectWithTag("Player")?.transform;
				if (_player != null) _lastPlayerPosition = _player.position;
			}
		}
	}
}