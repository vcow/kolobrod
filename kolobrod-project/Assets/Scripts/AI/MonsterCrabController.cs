using System.Collections.Generic;
using System.Linq;
using Common;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;

namespace AI
{
	[DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
	public class MonsterCrabController : MonsterControllerBase
	{
		private AudioSource _audioSource;
		private int _walkSoundId;

		private int _playerMask;
		private ContactFilter2D _contactFilter2D;

		private readonly List<Collider2D> _overlapped = new List<Collider2D>();

#pragma warning disable 649
		[SerializeField] private ParticleSystem _smog;
		[SerializeField] private GameObject _finalExplosionPrefab;
		[SerializeField] private float _decompositionDurationMin = 3f;
		[SerializeField] private float _decompositionDurationMax = 7f;
		[SerializeField] private float _explosionRadius = 5f;
#pragma warning restore 649

		protected override void OnWalkChanged(bool walk)
		{
			if (walk && _walkSoundId <= 0)
			{
				_walkSoundId = _audioManager.PlaySound("robot_walk", loopCount: 0, audioSource: _audioSource);
			}
			else if (!walk && _walkSoundId > 0)
			{
				_audioManager.StopSound(_walkSoundId);
				_walkSoundId = 0;
			}
		}

		protected override void OnAmmoHit()
		{
			_audioManager.PlaySound("bullet_hit");
		}

		protected override void UpdateDamagePercent(float damagePercent)
		{
			if (damagePercent > 0.3f)
			{
				if (!_smog.isPlaying) _smog.Play(true);

				var e = _smog.emission;
				e.rateOverTime = 25 * (damagePercent - 0.3f) / 0.7f;
			}
		}

		protected override void Start()
		{
			_audioSource = GetComponent<AudioSource>();

			_playerMask = LayerMask.GetMask("Player");
			_contactFilter2D = new ContactFilter2D {useTriggers = true, useLayerMask = true, layerMask = _playerMask};

			base.Start();
		}

		protected override void OnDestroy()
		{
			if (_walkSoundId > 0) _audioManager.StopSound(_walkSoundId);
			base.OnDestroy();
		}

		protected override void Die()
		{
			_smog.transform.SetParent(null, true);

			base.Die();

			if (_walkSoundId > 0)
			{
				_audioManager.StopSound(_walkSoundId);
				_walkSoundId = 0;
			}

			DOTween.To(() => Color.white, value =>
				{
					foreach (var meshInstance in MeshInstances)
					{
						meshInstance.color = value;
					}
				}, Color.grey,
				Random.Range(_decompositionDurationMin, _decompositionDurationMax)).OnComplete(() =>
			{
				var explosion = _diContainer.InstantiateComponentOnNewGameObject<BombController>();
				explosion.transform.position = _smog.transform.position;
				explosion.Radius = _explosionRadius;
				explosion.FxPrefab = _finalExplosionPrefab;
				explosion.Power = 50;

				_smog.GetComponent<PositionConstraint>().constraintActive = false;
				_smog.Stop(true);
				Destroy(_smog.gameObject, 15f);
				Destroy(gameObject);

				explosion.Detonate();

				if (_player && (explosion.transform.position - _playerTransform.position)
				    .sqrMagnitude <= _explosionRadius * _explosionRadius)
				{
					_player.Damage(Damage, null);
				}
			});
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

			_player.Damage(Damage, numPoints > 0 ? new Vector2(x / numPoints, y / numPoints) : (Vector2?) null);
		}
	}
}