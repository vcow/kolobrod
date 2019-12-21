using Base.AudioManager;
using UnityEngine;
using Zenject;

namespace AI
{
	[DisallowMultipleComponent]
	public class BombController : MonoBehaviour
	{
		private readonly Collider2D[] _affected = new Collider2D[32];

		public string SoundId = "explosion";
		public float Power = 10f;
		public float Radius = 5f;
		public GameObject FxPrefab;

#pragma warning disable 649
		[Inject] private readonly IAudioManager _audioManager;
#pragma warning restore 649

		public void Detonate(bool destroy = true)
		{
			var p = transform.position;
			var size = Physics2D.OverlapCircleNonAlloc(p, Radius, _affected);
			for (var i = 0; i < size; ++i)
			{
				var body = _affected[i].GetComponent<Rigidbody2D>();
				if (body == null) continue;
				body.AddForceAtPosition(new Vector2(Power, Power), p, ForceMode2D.Impulse);
			}

			if (FxPrefab)
			{
				var fx = Instantiate(FxPrefab, transform);
				if (destroy) Destroy(gameObject, 3f);
				else Destroy(fx, 3f);
			}
			else
			{
				if (destroy) Destroy(gameObject);
			}

			_audioManager.PlaySound(SoundId);
		}
	}
}