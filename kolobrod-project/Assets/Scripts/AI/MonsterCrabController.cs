using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Anima2D;
using Base.AudioManager;
using Common;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using Zenject;
using Random = UnityEngine.Random;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(Animator), typeof(AudioSource))]
	public class MonsterCrabController : MonoBehaviour
	{
		private Rigidbody2D[] _rigidBodies;
		private IkLimb2D[] _limbs;
		private Animator _animator;
		private AudioSource _audioSource;

		private readonly CompositeDisposable _handlers = new CompositeDisposable();
		private IDisposable _timerHandler;
		private IDisposable _playerHandler;

		private PlayerCharacterController _player;

		private bool _landing;

		private int _groundId;
		private int _characterId;
		private int _containerId;
		private int _playerId;
		private int _ammoId;

		private bool _aggred;

		private Transform _transform;
		private Transform _playerTransform;

		private float _velocity;

		private bool _walk;
		private bool _invert;
		private bool _attack;
		private bool _isDead;

		private static readonly int IsWalk = Animator.StringToHash("IsWalk");
		private static readonly int Attack = Animator.StringToHash("Attack");
		private static readonly int Hit = Animator.StringToHash("Hit");

		private readonly List<Collider2D> _overlapped = new List<Collider2D>();

		private int _playerMask;
		private ContactFilter2D _contactFilter2D;

		private float _initialHealth;
		private int _walkSoundId;

		private Coroutine _patrolRoutine;

#pragma warning disable 649
		[SerializeField] private Rigidbody2D _container;
		[SerializeField] private float _aggressDistance;
		[SerializeField] private float _attackMaxDistance;
		[SerializeField] private float _attackMinDistance;
		[SerializeField] private float _speed;
		[SerializeField] private float _rechargeTime;
		[SerializeField] private float _damage;
		[SerializeField] private FloatReactiveProperty _health;
		[SerializeField] private ParticleSystem _smog;
		[SerializeField] private GameObject _finalExplosionPrefab;
		[SerializeField] private float _decompositionDuration = 3f;
		[SerializeField] private SpriteMeshInstance _meshInstance;

		[Inject] private readonly IAudioManager _audioManager;
#pragma warning restore 649

		private bool Walk
		{
			get => _walk;
			set
			{
				if (value == _walk) return;
				_walk = value;
				if (_walk && _walkSoundId <= 0)
				{
					_walkSoundId = _audioManager.PlaySound("robot_walk", loopCount: 0, audioSource: _audioSource);
				}
				else if (!_walk && _walkSoundId > 0)
				{
					_audioManager.StopSound(_walkSoundId);
					_walkSoundId = 0;
				}
			}
		}

		private void Start()
		{
			_groundId = LayerMask.NameToLayer("Ground");
			_characterId = LayerMask.NameToLayer("Character");
			_containerId = LayerMask.NameToLayer("Container");
			_playerId = LayerMask.NameToLayer("Player");
			_ammoId = LayerMask.NameToLayer("Ammo");

			_initialHealth = _health.Value;

			_playerMask = LayerMask.GetMask("Player");
			_contactFilter2D = new ContactFilter2D {useTriggers = true, useLayerMask = true, layerMask = _playerMask};

			_transform = transform;
			_animator = GetComponent<Animator>();
			_audioSource = GetComponent<AudioSource>();

			_rigidBodies = GetComponentsInChildren<Rigidbody2D>();
			_limbs = GetComponentsInChildren<IkLimb2D>();
			foreach (var rigidBody in _rigidBodies)
			{
				rigidBody.bodyType = RigidbodyType2D.Kinematic;
				rigidBody.GetComponent<Collider2D>().isTrigger = true;
				_handlers.Add(rigidBody.OnTriggerEnter2DAsObservable().Subscribe(OnCollide));
			}
		}

		private void OnDestroy()
		{
			if (_patrolRoutine != null) StopCoroutine(_patrolRoutine);
			_handlers.Dispose();
			if (_walkSoundId > 0) _audioManager.StopSound(_walkSoundId);
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
			var vel = Random.value > 1 ? _speed : -_speed;
			for (;;)
			{
				yield return new WaitForSeconds(1f + Random.Range(0, 5));
				_velocity = vel;
				vel *= -1f;
				yield return new WaitForSeconds(2f + Random.Range(0, 10));
				_velocity = 0;
			}
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

				UpdateSmog();
				if (_health.Value <= 0) Die();

				_audioManager.PlaySound("bullet_hit");
			}
		}

		private void UpdateSmog()
		{
			var damagePercent = 1f - Mathf.Clamp01(_health.Value / _initialHealth);
			if (damagePercent > 0.3f)
			{
				if (!_smog.isPlaying) _smog.Play(true);

				var e = _smog.emission;
				e.rateOverTime = 25 * (damagePercent - 0.3f) / 0.7f;
			}
		}

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

		public void OnAttack()
		{
			if (!_player) return;

			var touchingParts = _rigidBodies.Where(rb => rb.IsTouchingLayers(_playerMask)).ToArray();
			if (touchingParts.Length <= 0) return;

			var x = 0f;
			var y = 0f;
			var numPoints = 0;
			foreach (var touchingPart in touchingParts)
			{
				var c = touchingPart.GetComponent<Collider2D>();
				Physics2D.OverlapCollider(c, _contactFilter2D, _overlapped);

				var points = _overlapped.Select(d =>
				{
					if (c.bounds.Intersection(d.bounds, out var intersection))
					{
						return (Vector2?) intersection.center;
					}

					return null;
				}).Where(v => v.HasValue).Select(v => v.Value).ToArray();

				numPoints += points.Length;
				foreach (var pt in points)
				{
					x += pt.x;
					y += pt.y;
				}
			}

			_player.Damage(_damage, numPoints > 0 ? new Vector2(x / numPoints, y / numPoints) : (Vector2?) null);
		}

		public void Die()
		{
			Assert.IsFalse(_isDead);

			if (_walkSoundId > 0)
			{
				_audioManager.StopSound(_walkSoundId);
				_walkSoundId = 0;
			}

			_isDead = true;
			transform.SetParent(null, true);
			_smog.transform.SetParent(null, true);
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

			DOTween.To(() => Color.white, value => _meshInstance.color = value, Color.grey, _decompositionDuration)
				.OnComplete(() =>
				{
					_smog.GetComponent<PositionConstraint>().constraintActive = false;
					_smog.Stop(true);
					Destroy(_smog.gameObject, 15f);

					_audioManager.PlaySound("explosion");
					var explosion = Instantiate(_finalExplosionPrefab, _smog.transform.position, Quaternion.identity);
					Destroy(explosion, 3f);

					Destroy(gameObject);
				});
		}
	}
}