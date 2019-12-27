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

		private float _aggressDistanceQuad;
		private bool _aggred;

		private Transform _transform;

		private float _velocity;

		private bool _isWalk;
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

			_aggressDistanceQuad = _aggressDistance * _aggressDistance;
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

			_collisionHandlers.Add(_container.OnCollisionEnter2DAsObservable().Subscribe(OnCollideContainer));
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
				return;
			}

			if (!_aggred)
			{
				_aggred = (_player.transform.position - _transform.position).sqrMagnitude <= _aggressDistanceQuad;
				return;
			}

			if (_isAttack)
			{
				_velocity = 0;
			}
			else
			{
				if (_player.transform.position.x < _transform.position.x)
				{
					if (_velocity >= 0)
					{
						_transform.localScale = new Vector3(1, 1, 1);
						_velocity = -_speed;
					}
				}
				else
				{
					if (_velocity < 0)
					{
						_transform.localScale = new Vector3(-1, 1, 1);
						_velocity = _speed;
					}
				}
			}

			if (!_isWalk && Mathf.Abs(_velocity) > 0)
			{
				_isWalk = true;
				_animator.SetBool(IsWalk, true);
			}

			_container.velocity = new Vector2(_velocity, _container.velocity.y);
		}

		private void OnCollide(Collider2D c)
		{
			_landing |= c.gameObject.layer == _groundId;

			Debug.Log("COLLIDE!");
		}

		private void OnCollideContainer(Collision2D c)
		{
			if (!_isAttack && c.gameObject.layer == _containerId)
			{
				if (c.gameObject.CompareTag("Player"))
				{
					AttackPlayer();
				}
			}
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