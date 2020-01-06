using System;
using System.Collections;
using System.Collections.Generic;
using Anima2D;
using Base.AudioManager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using Random = UnityEngine.Random;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Animator))]
	public abstract class MonsterControllerBase : MonoBehaviour
	{
		protected Rigidbody2D[] _rigidBodies;
		private IkLimb2D[] _limbs;
		private Animator _animator;

		private readonly CompositeDisposable _handlers = new CompositeDisposable();
		private IDisposable _timerHandler;
		private IDisposable _playerHandler;

		protected PlayerCharacterController _player;

		private bool _landing;

		private static int _sortingLayerCtr;

		private static readonly string[] _monstersSortingLayers = new[]
		{
			"CharacterSortingLayer1",
			"CharacterSortingLayer2",
			"CharacterSortingLayer3",
			"CharacterSortingLayer4",
			"CharacterSortingLayer5",
			"CharacterSortingLayer6",
			"CharacterSortingLayer7",
			"CharacterSortingLayer8",
			"CharacterSortingLayer9",
			"CharacterSortingLayer10",
		};

		private int _groundId;
		private int _ammoId;

		private bool _aggred;

		protected Transform _transform;
		protected Transform _playerTransform;

		private float _velocity;

		private bool _walk;
		private bool _invert;
		private bool _attack;
		private bool _isDead;

		private static readonly int IsWalk = Animator.StringToHash("IsWalk");
		private static readonly int Attack = Animator.StringToHash("Attack");
		private static readonly int Hit = Animator.StringToHash("Hit");

		private float _initialHealth;

		private Coroutine _patrolRoutine;

		private float _rnd = 0.5f;

#pragma warning disable 649
		[SerializeField] private Rigidbody2D _container;
		[SerializeField] private float _aggressDistance;
		[SerializeField] private float _attackMaxDistance;
		[SerializeField] private float _attackMinDistance;
		[SerializeField] private float _speed;
		[SerializeField] private float _rechargeTime;
		[SerializeField] private float _damage;
		[SerializeField] private FloatReactiveProperty _health;
		[SerializeField] private SpriteMeshInstance[] _meshInstances = new SpriteMeshInstance[0];
		[SerializeField] private ParticleSystem[] _particleSystems = new ParticleSystem[0];
#pragma warning restore 649

		[Inject] protected readonly DiContainer _diContainer;
		[Inject] protected readonly IAudioManager _audioManager;

		protected float Damage => _damage;

		protected IReadOnlyList<SpriteMeshInstance> MeshInstances => _meshInstances;

		private bool Walk
		{
			get => _walk;
			set
			{
				if (value == _walk) return;
				_walk = value;
				OnWalkChanged(_walk);
			}
		}

		protected abstract void OnWalkChanged(bool walk);

		protected virtual void Start()
		{
			var sortingLayer = _monstersSortingLayers[_sortingLayerCtr++ % _monstersSortingLayers.Length];
			foreach (var meshInstance in _meshInstances)
			{
				meshInstance.sortingLayerName = sortingLayer;
			}

			foreach (var ps in _particleSystems)
			{
				ps.GetComponent<Renderer>().sortingLayerName = sortingLayer;
			}

			_groundId = LayerMask.NameToLayer("Ground");
			_ammoId = LayerMask.NameToLayer("Ammo");

			_initialHealth = _health.Value;

			_transform = transform;
			_animator = GetComponent<Animator>();

			_rigidBodies = GetComponentsInChildren<Rigidbody2D>();
			_limbs = GetComponentsInChildren<IkLimb2D>();
			foreach (var rigidBody in _rigidBodies)
			{
				rigidBody.bodyType = RigidbodyType2D.Kinematic;
				rigidBody.GetComponent<Collider2D>().isTrigger = true;
				_handlers.Add(rigidBody.OnTriggerEnter2DAsObservable().Subscribe(OnCollide));
			}
		}

		protected virtual void OnDestroy()
		{
			if (_patrolRoutine != null) StopCoroutine(_patrolRoutine);
			_handlers.Dispose();
		}

		private void FixedUpdate()
		{
			if (!_landing || _isDead) return;

			if (!_player)
			{
				if (Walk)
				{
					Walk = false;
					_animator.SetBool(IsWalk, false);
				}

				_player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerCharacterController>();
				if (_player != null)
				{
					Assert.IsNull(_playerTransform);
					Assert.IsNull(_playerHandler);
					_playerTransform = _player.transform;
					_playerHandler = _player.OnDestroyAsObservable().First().Subscribe(unit =>
					{
						_handlers.Remove(_playerHandler);
						_playerHandler = null;
						_player = null;
						_playerTransform = null;
						_aggred = false;
					});
					_handlers.Add(_playerHandler);
					return;
				}

				if (_patrolRoutine == null)
				{
					_patrolRoutine = StartCoroutine(PatrolRoutine());
				}
			}
			else
			{
				var magnitude = Mathf.Abs((_playerTransform.position - _transform.position).x);
				_aggred |= magnitude <= _aggressDistance;

				if (_aggred)
				{
					if (!_attack && !_player.IsDead && magnitude <= _attackMaxDistance &&
					    magnitude > _attackMinDistance)
					{
						AttackPlayer();
					}

					if (_attack || _player.IsDead)
					{
						_velocity = 0;
					}
					else
					{
						_velocity = _playerTransform.position.x < _transform.position.x ? -_speed : _speed;
						if (magnitude <= _attackMinDistance) _velocity *= -1;
					}

					if (_patrolRoutine != null)
					{
						StopCoroutine(_patrolRoutine);
						_patrolRoutine = null;
					}
				}
				else if (_patrolRoutine == null)
				{
					_patrolRoutine = StartCoroutine(PatrolRoutine());
				}
			}

			var isWalk = Mathf.Abs(_velocity) > 0;
			if (Walk != isWalk)
			{
				Walk = isWalk;
				_animator.SetBool(IsWalk, Walk);
			}

			var invert = Walk ? _velocity > 0 : _invert;
			if (invert != _invert)
			{
				_invert = invert;
				_transform.localScale = _invert ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
			}

			_container.velocity = new Vector2(_velocity, _container.velocity.y);
		}

		private IEnumerator PatrolRoutine()
		{
			var rnd = Random.value;
			var vel = rnd > _rnd ? _speed : -_speed;
			_rnd = rnd;

			for (;;)
			{
				yield return new WaitForSeconds(Random.Range(0.5f, 5f));
				_velocity = vel;
				vel *= -1f;
				yield return new WaitForSeconds(Random.Range(1, 10));
				_velocity = 0;
			}

			// ReSharper disable once IteratorNeverReturns
		}

		private void OnCollide(Collider2D c)
		{
			_landing |= c.gameObject.layer == _groundId;

			if (c.gameObject.layer == _ammoId)
			{
				_aggred = true;

				_animator.SetTrigger(Hit);
				var ammo = c.GetComponent<AmmoController>();
				_health.SetValueAndForceNotify(_health.Value - ammo.Damage);

				UpdateDamagePercent(1f - Mathf.Clamp01(_health.Value / _initialHealth));
				if (_health.Value <= 0) Die();

				OnAmmoHit();
			}
		}

		protected abstract void OnAmmoHit();

		protected abstract void UpdateDamagePercent(float damagePercent);

		private void AttackPlayer()
		{
			Assert.IsNotNull(_player);
			Assert.IsNull(_timerHandler);
			Assert.IsFalse(_attack);

			_attack = true;

			var invert = _playerTransform.position.x > _transform.position.x;
			if (invert != _invert)
			{
				_invert = invert;
				_transform.localScale = _invert ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
			}

			_animator.SetTrigger(Attack);
			_timerHandler = Observable.Timer(TimeSpan.FromSeconds(_rechargeTime)).Subscribe(l =>
			{
				_handlers.Remove(_timerHandler);
				_timerHandler = null;
				_attack = false;
			});
			_handlers.Add(_timerHandler);
		}

		protected virtual void Die()
		{
			Assert.IsFalse(_isDead);

			_isDead = true;
			transform.SetParent(null, true);
			Destroy(_container.gameObject);

			_animator.enabled = false;

			_handlers.Dispose();
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