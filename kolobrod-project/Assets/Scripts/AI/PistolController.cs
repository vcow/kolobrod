using DG.Tweening;
using UnityEngine;

namespace AI
{
	public class PistolController : WeaponController
	{
#pragma warning disable 649
		[SerializeField] private ParticleSystem _fireFx;
#pragma warning restore 649

		public override bool Shut(Vector2 targetPoint)
		{
			if (_fireFx.isPlaying) _fireFx.Stop(true);
			_fireFx.Play(true);
			return true;
		}
	}
}