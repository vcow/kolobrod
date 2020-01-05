using UnityEngine;
using UnityEngine.Animations;

namespace AI
{
	public class MonsterTibaltController : MonsterControllerBase
	{
#pragma warning disable 649
		[SerializeField] private ParticleSystem _flame;
#pragma warning restore 649

		protected override void OnWalkChanged(bool walk)
		{
		}

		protected override void OnAmmoHit()
		{
		}

		protected override void UpdateDamagePercent(float damagePercent)
		{
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
		}
	}
}