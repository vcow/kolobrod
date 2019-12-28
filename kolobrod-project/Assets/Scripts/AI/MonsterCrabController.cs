using System;
using System.Collections.Generic;
using System.Linq;
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

		private readonly CompositeDisposable _handlers = new CompositeDisposable();
		private IDisposable _timerHandler;
		private IDisposable _playerHandler;

		private PlayerCharacterController _player;

		private bool _landing;

		private int _groundId;
		private int _characterId;
		private int _containerId;
		private int _playerId;

		private bool _aggred;

		private Transform _transform;
		private Transform _playerTransform;

		private float _velocity;

		private bool _isWalk;
		private bool _invert;
		private bool _isAttack;

		private static readonly int IsWalk = Animator.StringToHash("IsWalk");
		private static readonly int Attack = Animator.StringToHash("Attack");

		private readonly List<Collider2D> _overlapped = new List<Collider2D>();

		private int _playerMask;
		private ContactFilter2D _contactFilter2D;

#pragma warning disable 649
		[SerializeField] private Rigidbody2D _container;
		[SerializeField] private float _aggressDistance;
		[SerializeField] private float _attackMaxDistance;
		[SerializeField] private float _attackMinDistance;
		[SerializeField] private float _speed;
		[SerializeField] private float _rechargeTime;
		[SerializeField] private float _damage;
		[SerializeField] private float _health;
#pragma warning restore 649

		private void Start()
		{
			_groundId = LayerMask.NameToLayer("Ground");
			_characterId = LayerMask.NameToLayer("Character");
			_containerId = LayerMask.NameToLayer("Container");
			_playerId = LayerMask.NameToLayer("Player");

			_playerMask = LayerMask.GetMask("Player");
			_contactFilter2D = new ContactFilter2D {useTriggers = true, useLayerMask = true, layerMask = _playerMask};

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

		private void OnDestroy()
		{
			_handlers.Dispose();
		}

		private void FixedUpdate()
		{
			if (!_landing) return;

			if (!_player)
			{
				_velocity = 0;

				if (_isWalk)
				{
					_isWalk = false;
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
					});
					_handlers.Add(_playerHandler);
				}

				return;
			}

			var magnitude = Mathf.Abs((_playerTransform.position - _transform.position).x);
			if (!_aggred)
			{
				_aggred = magnitude <= _aggressDistance;
				return;
			}

			if (!_isAttack && !_player.IsDead && magnitude <= _attackMaxDistance && magnitude > _attackMinDistance)
			{
				AttackPlayer();
			}

			if (_isAttack || _player.IsDead)
			{
				_velocity = 0;
			}
			else
			{
				_velocity = _playerTransform.position.x < _transform.position.x ? -_speed : _speed;
				if (magnitude <= _attackMinDistance) _velocity *= -1;
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
		}

		private void AttackPlayer()
		{
			Assert.IsNotNull(_player);
			Assert.IsNull(_timerHandler);
			Assert.IsFalse(_isAttack);

			_isAttack = true;

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
				_isAttack = false;
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
					var rc = new Rect
					{
						min = new Vector2(
							Mathf.Max(d.bounds.min.x, c.bounds.min.x),
							Mathf.Max(d.bounds.min.y, c.bounds.min.y)
						),
						max = new Vector2(
							Mathf.Min(d.bounds.max.x, c.bounds.max.x),
							Mathf.Min(d.bounds.max.y, c.bounds.max.y)
						)
					};
					return rc.center;
				}).ToArray();

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