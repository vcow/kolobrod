using System;
using System.Collections;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace AI
{
	[DisallowMultipleComponent]
	public class MonsterSpawnZone : MonoBehaviour
	{
		private readonly CompositeDisposable _handlers = new CompositeDisposable();
		private IDisposable _spawnTimerHandler;
		private int _monstersPresent;

#pragma warning disable 649
		[SerializeField] private int _numMonsters = 3;
		[SerializeField] private float _spawnDelayTime = 2;
		[SerializeField] private GameObject _monsterPrefab;
		[SerializeField] private float _initialScale;
		[SerializeField] private Vector2 _initialForce;

		[Inject] private readonly DiContainer _container;
#pragma warning restore 649

		private void Start()
		{
			StartCoroutine(Starter());
		}

		private void OnDestroy()
		{
			_handlers.Dispose();
		}

		private IEnumerator Starter()
		{
			yield return new WaitForSeconds(2);
			Spawn();
		}

		private void Spawn()
		{
			Assert.IsNull(_spawnTimerHandler);

			var monster = Instantiate(_monsterPrefab, transform.position, Quaternion.identity);
			_container?.InjectGameObject(monster);
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

			IDisposable h = null;
			h = monster.OnDestroyAsObservable().Subscribe(unit =>
			{
				// ReSharper disable once AccessToModifiedClosure
				_handlers.Remove(h);
				--_monstersPresent;
				if (_monstersPresent < _numMonsters && _spawnTimerHandler == null)
				{
					Spawn();
				}
			});
			_handlers.Add(h);

			++_monstersPresent;
			_spawnTimerHandler = Observable.Timer(TimeSpan.FromSeconds(_spawnDelayTime)).ObserveOnMainThread()
				.Subscribe(l =>
				{
					_handlers.Remove(_spawnTimerHandler);
					_spawnTimerHandler = null;
					if (_monstersPresent < _numMonsters)
					{
						Spawn();
					}
				});
			_handlers.Add(_spawnTimerHandler);
		}
	}
}