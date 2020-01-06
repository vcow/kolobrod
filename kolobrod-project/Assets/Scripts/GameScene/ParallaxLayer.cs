using UnityEngine;

namespace GameScene
{
	[DisallowMultipleComponent]
	public class ParallaxLayer : MonoBehaviour
	{
		private Vector2 _baseObjectInitialPosition;
		private Vector3 _initialPosition;
		private Transform _transform;

#pragma warning disable 649
		[SerializeField] private Transform _baseObject;
		[SerializeField] private float _parallax;
#pragma warning restore 649

		private void Start()
		{
			_transform = transform;
			_initialPosition = _transform.position;
			_baseObjectInitialPosition = _baseObject.position;
		}

		private void Update()
		{
			_transform.position =
				_initialPosition + (Vector3) ((Vector2) _baseObject.position - _baseObjectInitialPosition) * _parallax;
		}
	}
}