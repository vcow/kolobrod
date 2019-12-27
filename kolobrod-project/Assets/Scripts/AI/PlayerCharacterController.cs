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

		private Vector2 _movement;
		private bool _walk;
		private bool _reverse;
		private Camera _cam;

		private static readonly int IsWalk = Animator.StringToHash("IsWalk");
		private static readonly int WeaponUp = Animator.StringToHash("WeaponUp");
		private static readonly int Recoil = Animator.StringToHash("Recoil");

		private float _shotAng;

#pragma warning disable 649
		[SerializeField] private float _speed = 100;
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
			_movement = value * Vector2.right * _speed;
		}

		private void FixedUpdate()
		{
			_rigidBody.AddForce(_movement);
			var vel = _rigidBody.velocity.x;
			var walk = Mathf.Abs(vel) > 0;
			var reverse = vel < 0;
			if (walk != _walk)
			{
				_animator.SetBool(IsWalk, walk);
				_walk = walk;
			}

			if (walk && reverse != _reverse)
			{
				_transform.localScale = reverse ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
				_reverse = reverse;
			}
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
	}
}