using Anima2D;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Assertions;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Animator))]
	public class MonsterCrabController : MonoBehaviour
	{
		private Rigidbody2D[] _rigidBodies;
		private IkLimb2D[] _limbs;
		private Animator _animator;

		private readonly CompositeDisposable _collisionHandlers = new CompositeDisposable();

		private PlayerCharacterController _player;

		private bool _landing;

		private int _groundId;
		private int _characterId;
		private int _containerId;

		private bool _aggred;

		private Transform _transform;
		private Transform _playerTransform;

		private float _velocity;

		private bool _isWalk;
		private bool _invert;
		private bool _isAttack;

		private static readonly int IsWalk = Animator.StringToHash("IsWalk");
		private static readonly int Attack = Animator.StringToHash("Attack");

#pragma warning disable 649
		[SerializeField] private Rigidbody2D _container;
		[SerializeField] private float _aggressDistance;
		[SerializeField] private float _speed;
#pragma warning restore 649

		private void Start()
		{
			_groundId = LayerMask.NameToLayer("Ground");
			_characterId = LayerMask.NameToLayer("Character");
			_containerId = LayerMask.NameToLayer("Container");

			_transform = transform;
			_animator = GetComponent<Animator>();

			_rigidBodies = GetComponentsInChildren<Rigidbody2D>();
			_limbs = GetComponentsInChildren<IkLimb2D>();
			foreach (var rigidBody in _rigidBodies)
			{
				rigidBody.bodyType = RigidbodyType2D.Kinematic;
				rigidBody.GetComponent<Collider2D>().isTrigger = true;
				_collisionHandlers.Add(rigidBody.OnTriggerEnter2DAsObservable().Subscribe(OnCollide));
			}
		}

		private void OnDestroy()
		{
			_collisionHandlers.Dispose();
		}

		private void FixedUpdate()
		{
			if (!_landing) return;

			if (!_player)
			{
				if (Mathf.Abs(_velocity) > 0)
				{
					_velocity = 0;
				}

				if (_isWalk)
				{
					_isWalk = false;
					_animator.SetBool(IsWalk, false);
				}

				_player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerCharacterController>();
				_playerTransform = _player != null ? _player.transform : null;
				return;
			}

			var magnitude = Mathf.Abs((_playerTransform.position - _transform.position).x);
			if (!_aggred)
			{
				_aggred = magnitude <= _aggressDistance;
				return;
			}

			if (_isAttack || magnitude <= 0.1f)
			{
				_velocity = 0;
			}
			else
			{
				_velocity = _playerTransform.position.x < _transform.position.x ? -_speed : _speed;
			}

			var isWalk = Mathf.Abs(_velocity) > 0;
			if (_isWalk != isWalk)
			{
				_isWalk = isWalk;
				_animator.SetBool(IsWalk, _isWalk);
			}

			var invert = _isWalk ? _velocity > 0 : _invert;
			if (invert != _invert)
			{
				_invert = invert;
				_transform.localScale = _invert ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
			}

			_container.velocity = new Vector2(_velocity, _container.velocity.y);
		}

		private void OnCollide(Collider2D c)
		{
			_landing |= c.gameObject.layer == _groundId;

//			Debug.Log("COLLIDE!");
		}

		private void AttackPlayer()
		{
			Assert.IsNotNull(_player);
			Assert.IsFalse(_isAttack);
			_isAttack = true;
			_animator.SetTrigger(Attack);
		}

		public void OnAttack()
		{
		}

		public void Die()
		{
			transform.SetParent(null, true);
			Destroy(_container.gameObject);

			_collisionHandlers.Dispose();
			foreach (var rigidBody in _rigidBodies)
			{
				rigidBody.bodyType = RigidbodyType2D.Dynamic;
				rigidBody.GetComponent<Collider2D>().isTrigger = false;
			}

			foreach (var limb in _limbs)
			{
				limb.gameObject.SetActive(false);
			}
		}
	}
}