using UniRx;
using UnityEngine;

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
		private static readonly int Hit = Animator.StringToHash("Hit");

		private float _velocity;
		private Vector3 _aimPoint;

		private WeaponController _weapon;

		private readonly Subject<Vector2> _shutStream = new Subject<Vector2>();

		private readonly CompositeDisposable _handlers = new CompositeDisposable();

#pragma warning disable 649
		[SerializeField] private float _speed = 5;
		[SerializeField] private Transform _armRotationAxis;
		[SerializeField] private FloatReactiveProperty _health;
		[SerializeField] private GameObject _bloodSplatPrefab;
		[SerializeField] private Transform _weaponConnectionPoint;
		[SerializeField] private GameObject _defaultWeaponPrefab;
#pragma warning restore 649

		private void Awake()
		{
			_animator = GetComponent<Animator>();
			_rigidBody = GetComponent<Rigidbody2D>();
			_transform = transform;
			_cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

			_handlers.Add(_shutStream.ThrottleFrame(1).Subscribe(OnShut));
		}

		private void OnDestroy()
		{
			_handlers.Dispose();
		}

		private void OnShut(Vector2 worldPosition)
		{
			if (_weapon && _weapon.Shut(worldPosition))
			{
				_animator.SetTrigger(Recoil);
			}
		}

		private void Start()
		{
			if (_defaultWeaponPrefab)
			{
				_weapon = Instantiate(_defaultWeaponPrefab, _weaponConnectionPoint)
					.GetComponent<WeaponController>();
			}
		}

		public void Walk(Vector2 direction)
		{
			_velocity = IsDead ? 0 : direction.x * _speed;
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

		public void Aim(Vector2 point)
		{
			if (_weapon == null || !_weapon.WeaponIsReady || IsDead)
			{
				return;
			}

			var axisPosition = _armRotationAxis.position;
			_aimPoint = _cam.ScreenToWorldPoint(new Vector3(point.x, point.y, axisPosition.z));

			if (axisPosition.x > _aimPoint.x && !_invert ||
			    axisPosition.x <= _aimPoint.x && _invert)
			{
				return;
			}

			var vec = _aimPoint - axisPosition;
			var ang = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
			if (Mathf.Abs(ang) > 90f)
			{
				var a = 90f - (ang - 90f);
				ang = ang < 0 ? a - 360f : a;
			}

			_animator.SetFloat(WeaponUp, Mathf.Clamp01((ang + 90f) / 180f));
		}

		public void Fire()
		{
			if (_weapon == null || !_weapon.WeaponIsReady || IsDead)
			{
				return;
			}

			_shutStream.OnNext(_aimPoint);
		}

		public bool IsDead => _health.Value <= 0;

		public void Damage(float damage, Vector2? point)
		{
			_health.SetValueAndForceNotify(_health.Value - damage);
			if (damage > 0)
			{
				_animator.SetTrigger(Hit);
			}

			if (point.HasValue && _bloodSplatPrefab != null)
			{
				var blood = Instantiate(_bloodSplatPrefab, point.Value, Quaternion.identity);
				if (point.Value.x < _transform.position.x)
				{
					var s = blood.transform.localScale;
					blood.transform.localScale = new Vector3(s.x * -1, s.y, s.z);
				}

				Destroy(blood, 2);
			}

			if (_health.Value <= 0)
			{
				_animator.SetFloat(WeaponUp, 0);
			}
		}

		public IReadOnlyReactiveProperty<float> Health => _health;
	}
}