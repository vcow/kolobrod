using Base.AudioManager;
using UnityEngine;
using Zenject;

namespace AI
{
	public class PistolController : WeaponController
	{
#pragma warning disable 649
		[SerializeField] private ParticleSystem _fireFx;

		[Inject] private readonly IAudioManager _audioManager;
#pragma warning restore 649

		protected override bool DoShut(Vector2 targetPoint)
		{
			var bullet = Instantiate(_ammoPrefab).GetComponent<AmmoController>();
			var spawnPosition = _spawnPoint.position;
			bullet.transform.position = spawnPosition;
			bullet.Body.AddForce((targetPoint - (Vector2) spawnPosition).normalized * 10f, ForceMode2D.Impulse);

			if (_fireFx.isPlaying) _fireFx.Stop(true);
			_fireFx.Play(true);

			_audioManager.PlaySound("pistol_shut");
			return true;
		}
	}
}