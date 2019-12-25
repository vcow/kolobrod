using UnityEngine;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
	public class MainCameraController : MonoBehaviour
	{
		private Transform _player;
		private Transform _transform;
		private Vector3 _initialPosition;

		private void Start()
		{
			_transform = transform;
			_initialPosition = _transform.position;
		}

		private void Update()
		{
			if (_player) _transform.position = new Vector3(_player.position.x, _initialPosition.y, _initialPosition.z);
			else _player = GameObject.FindGameObjectWithTag("Player")?.transform;
		}
	}
}