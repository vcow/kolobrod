using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations;

namespace AI
{
	public class MonsterTibaltController : MonsterControllerBase
	{
#pragma warning disable 649
		[SerializeField] private ParticleSystem _flame;
		[SerializeField] private ParticleSystem _smoke;
		[SerializeField] private ParticleSystem _fire;
#pragma warning restore 649

		protected override void OnWalkChanged(bool walk)
		{
		}

		protected override void OnAmmoHit()
		{
		}

		protected override void UpdateDamagePercent(float damagePercent)
		{
			if (damagePercent > 0.5f)
			{
				if (!_fire.isPlaying)
				{
					_fire.Play(true);
					_smoke.Stop(true);
				}

				var e = _fire.emission;
				e.rateOverTime = 5 * (damagePercent - 0.5f) / 0.5f;
			}
		}

		public void OnStartAttack()
		{
			_flame.Play(true);
		}

		public void OnFinishAttack()
		{
			_flame.Stop(true);
		}

		protected override void Die()
		{
			base.Die();

			_flame.GetComponent<PositionConstraint>().constraintActive = false;
			_flame.Stop(true);

			if (!_smoke.isPlaying) _smoke.Play(true);

			var e1 = _fire.emission;
			var e2 = _smoke.emission;
			DOTween.Sequence()
				.Append(DOTween.To(() => 5, value => e1.rateOverTime = value, 100, 2).SetEase(Ease.InQuad))
				.Join(DOTween.To(() => 3, value => e2.rateOverTime = value, 7, 2).SetEase(Ease.Linear))
				.Append(DOTween.To(() => Color.white, value =>
				{
					foreach (var meshInstance in MeshInstances)
					{
						meshInstance.color = value;
					}
				}, new Color(1, 1, 1, 0), 1).SetDelay(1))
				.Append(DOTween.To(() => 100, value => e1.rateOverTime = value, 0, 1).SetEase(Ease.InQuad))
				.Join(DOTween.To(() => 7, value => e2.rateOverTime = value, 0, 1).SetEase(Ease.Linear))
				.OnComplete(() =>
				{
					foreach (var rigidBody in _rigidBodies)
					{
						rigidBody.simulated = false;
					}

					_fire.Stop(true);
					Destroy(gameObject, 5);
				});
		}
	}
}