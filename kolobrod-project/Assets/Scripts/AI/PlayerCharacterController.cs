using UnityEngine;
using UnityEngine.InputSystem;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
	public class PlayerCharacterController : MonoBehaviour
	{
		private Animator _animator;
		private Rigidbody2D _rigidBody;
		private Transform _transform;

		private bool _isWalk;
		private bool _invert;
		private Camera _cam;

		private static readonly int IsWalk = Animator.StringToHash("IsWalk");
		private static readonly int WeaponUp = Animator.StringToHash("WeaponUp");
		private static readonly int Recoil = Animator.StringToHash("Recoil");

		private float _shotAng;
		private float _velocity;

		private float _health = 100f;

#pragma warning disable 649
		[SerializeField] private float _speed = 5;
		[SerializeField] private Transform _armRotationAxis;
#pragma warning restore 649

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			_rigidBody = GetComponent<Rigidbody2D>();
			_transform = transform;
			_cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		}

		public void OnWalk(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;
			var value = context.ReadValue<Vector2>();
			_velocity = value.x * _speed;
		}

		private void FixedUpdate()
		{
			var isWalk = Mathf.Abs(_velocity) > 0;
			if (isWalk != _isWalk)
			{
				_isWalk = isWalk;
				_animator.SetBool(IsWalk, _isWalk);
			}

			var invert = _isWalk ? _velocity < 0 : _invert;
			if (invert != _invert)
			{
				_invert = invert;
				_transform.localScale = _invert ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
			}

			_rigidBody.velocity = new Vector2(_velocity, _rigidBody.velocity.y);
		}

		public void OnFire(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;
			var value = context.ReadValue<Vector2>();
			var axisPosition = _armRotationAxis.position;
			var worldPosition = _cam.ScreenToWorldPoint(new Vector3(value.x, value.y, axisPosition.z));
			var vec = worldPosition - axisPosition;
			var ang = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
			if (Mathf.Abs(ang) > 90f)
			{
				var a = 90f - (ang - 90f);
				ang = ang < 0 ? a - 360f : a;
			}

			_animator.SetFloat(WeaponUp, Mathf.Clamp01((ang + 90f) / 180f));
			_animator.SetTrigger(Recoil);
			_shotAng = ang;
		}

		public bool IsDead => _health <= 0;
	}
}