using Anima2D;
using Base.AudioManager;
using Common;
using GameScene.Signals;
using Helpers.TouchHelper;
using UniRx;
using UnityEngine;
using Zenject;

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
		private bool _isNavigate;

		private WeaponController _weapon;

		private readonly Subject<Vector2> _shutStream = new Subject<Vector2>();

		private readonly CompositeDisposable _handlers = new CompositeDisposable();

		private readonly FloatReactiveProperty _currentHealth = new FloatReactiveProperty();

		private SpriteMeshInstance[] _meshes;
		private Color _color = Color.white;

#pragma warning disable 649
		[SerializeField] private float _speed = 5;
		[SerializeField] private float _health = 1000;
		[SerializeField] private Transform _armRotationAxis;
		[SerializeField] private GameObject _bloodSplatPrefab;
		[SerializeField] private Transform _weaponConnectionPoint;
		[SerializeField] private Transform _audioListenerConnectionPoint;
		[SerializeField] private GameObject _defaultWeaponPrefab;

		[Inject] private readonly DiContainer _container;
		[Inject] private readonly SignalBus _signalBus;
		[Inject] private readonly AudioListenerController _audioListenerController;
		[Inject] private readonly IAudioManager _audioManager;
#pragma warning restore 649

		private void Awake()
		{
			_currentHealth.SetValueAndForceNotify(_health);

			_animator = GetComponent<Animator>();
			_rigidBody = GetComponent<Rigidbody2D>();
			_transform = transform;
			_cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

			_handlers.Add(_shutStream.ThrottleFrame(1).Subscribe(OnShut));

			_meshes = GetComponentsInChildren<SpriteMeshInstance>();
		}

		private void OnDestroy()
		{
			_signalBus.Unsubscribe<MoveSignal>(OnMove);
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
			_signalBus.Subscribe<MoveSignal>(OnMove);
			_audioListenerController.Pursued = _audioListenerConnectionPoint;

			if (_defaultWeaponPrefab)
			{
				_weapon = _container.InstantiatePrefabForComponent<WeaponController>(_defaultWeaponPrefab,
					_weaponConnectionPoint);
				_weapon.Color = _color;
			}
		}

		private void Update()
		{
			Touch touch;
			if (_isNavigate && TouchHelper.GetTouch(out touch, 1) && touch.phase == TouchPhase.Began ||
			    !TouchHelper.IsPointerOverUiObject() && TouchHelper.GetTouch(out touch) &&
			    touch.phase == TouchPhase.Began)
			{
				Fire(touch.position);
			}

#if UNITY_EDITOR
			if (Input.GetButton("Horizontal"))
			{
				Walk(Input.GetAxis("Horizontal") * Vector2.right);
			}
			else if (Input.GetButton("Vertical"))
			{
				Walk(Input.GetAxis("Vertical") * Vector2.up);
			}
			else if (!_isNavigate)
			{
				Walk(Vector2.zero);
			}
#endif
		}

		private void OnMove(MoveSignal signal)
		{
			_isNavigate = signal.Direction != Vector2.zero;
			Walk(signal.Direction);
		}

		private void Walk(Vector2 direction)
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

		private void Fire(Vector2 point)
		{
			if (!_weapon || !_weapon.WeaponIsReady || IsDead)
			{
				return;
			}

			var axisPosition = _armRotationAxis.position;
			var worldPosition = _cam.ScreenToWorldPoint(new Vector3(point.x, point.y, axisPosition.z));

			if (axisPosition.x > worldPosition.x && !_invert ||
			    axisPosition.x <= worldPosition.x && _invert)
			{
				return;
			}

			var vec = worldPosition - axisPosition;
			var ang = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
			if (Mathf.Abs(ang) > 90f)
			{
				var a = 90f - (ang - 90f);
				ang = ang < 0 ? a - 360f : a;
			}

			_animator.SetFloat(WeaponUp, Mathf.Clamp01((ang + 90f) / 180f));
			_shutStream.OnNext(worldPosition);
		}

		public bool IsDead => _currentHealth.Value <= 0;

		public void Damage(float damage, Vector2? point)
		{
			if (IsDead) return;

			_currentHealth.SetValueAndForceNotify(_currentHealth.Value - damage);
			if (damage > 0)
			{
				_animator.SetTrigger(Hit);
				_audioManager.PlaySound("kick_player");
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

			if (_currentHealth.Value <= 0)
			{
				_animator.SetFloat(WeaponUp, 0);
			}
		}

		public IReadOnlyReactiveProperty<float> CurrentHealth => _currentHealth;

		public float Health => _health;

		public Color Color
		{
			set
			{
				_color = value;
				foreach (var mesh in _meshes)
				{
					mesh.color = _color;
				}

				if (_weapon) _weapon.Color = _color;
			}
		}
	}
}