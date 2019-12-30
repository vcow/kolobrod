using System.Collections;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace AI
{
	[DisallowMultipleComponent]
	public class MonsterSpawnZone : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField] private GameObject _monsterPrefab;
		[SerializeField] private float _initialScale;
		[SerializeField] private Vector2 _initialForce;

		[Inject] private readonly DiContainer _container;
#pragma warning restore 649

		private void Start()
		{
			StartCoroutine(Starter());
		}

		private IEnumerator Starter()
		{
			yield return new WaitForSeconds(2);
			Spawn();
		}

		public void Spawn()
		{
			var monster = Instantiate(_monsterPrefab, transform.position, Quaternion.identity);
			_container?.Inject(monster);
			monster.transform.localScale = new Vector3(_initialScale, _initialScale, _initialScale);
			if (_initialScale < 1)
			{
				monster.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad);
			}

			if (_initialForce != Vector2.zero)
			{
				var body = monster.GetComponent<Rigidbody2D>();
				if (body != null)
				{
					body.FixedUpdateAsObservable().First().Subscribe(unit =>
					{
						body.AddRelativeForce(_initialForce, ForceMode2D.Impulse);
					});
				}
			}
		}
	}
}