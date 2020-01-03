using System;
using Common;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace AI
{
	[DisallowMultipleComponent]
	public class CharacterSpawnZone : MonoInstaller<CharacterSpawnZone>
	{
		private PlayerCharacterController _character;
		private readonly IntReactiveProperty _livesLeft = new IntReactiveProperty();
		private readonly FloatReactiveProperty _health = new FloatReactiveProperty();
		private readonly CompositeDisposable _handlers = new CompositeDisposable();

#pragma warning disable 649
		[SerializeField] private int _numLives;
		[SerializeField] private Transform _spawnPosition;
		[SerializeField] private GameObject _appearFx;
		[SerializeField] private GameObject _disappearFx;

		[Inject] private readonly AvatarType _avatarType;
#pragma warning restore 649

		public override void InstallBindings()
		{
			Container.Bind<CharacterSpawnZone>().FromInstance(this).AsCached();
		}

		public override void Start()
		{
			_livesLeft.SetValueAndForceNotify(_numLives);
			Spawn();
		}

		private void OnDestroy()
		{
			_handlers.Dispose();
		}

		public int NumLives => _numLives;

		public IReadOnlyReactiveProperty<int> LivesLeft => _livesLeft;

		public IReadOnlyReactiveProperty<float> Health => _health;

		private void Spawn()
		{
			Assert.IsNull(_character);
			Assert.IsTrue(_livesLeft.Value > 0);

			var fx = Instantiate(_appearFx);
			fx.transform.position = new Vector3(_spawnPosition.position.x, 0, 0);
			Destroy(fx, 3f);

			switch (_avatarType)
			{
				case AvatarType.Anna:
					_character = Container.InstantiatePrefabResourceForComponent<PlayerCharacterController>(
						"Characters/Anna");
					break;
				case AvatarType.Antonio:
					_character = Container.InstantiatePrefabResourceForComponent<PlayerCharacterController>(
						"Characters/Antonio");
					break;
				default:
					throw new NotSupportedException();
			}

			_character.transform.position = new Vector3(_spawnPosition.position.x, 0, 0);
			_character.Color = new Color(1, 1, 1, 0);

			_handlers.Add(_character.CurrentHealth.Subscribe(f =>
			{
				_health.SetValueAndForceNotify(Mathf.Clamp01(f / _character.Health));
				if (_character.IsDead) RespawnCharacter();
			}));

			DOTween.To(() => new Color(1, 1, 1, 0), value => _character.Color = value, Color.white, 1f);
		}

		private void RespawnCharacter()
		{
			Assert.IsNotNull(_character);

			var fx = Instantiate(_disappearFx);
			fx.transform.position = _character.transform.position;
			Destroy(fx, 3f);

			_handlers.Clear();

			DOTween.To(() => Color.white, value => _character.Color = value, new Color(1, 1, 1, 0), 0.5f)
				.OnComplete(() =>
				{
					Destroy(_character.gameObject);
					_character = null;
					_livesLeft.SetValueAndForceNotify(_livesLeft.Value - 1);
					if (_livesLeft.Value > 0)
					{
						Observable.Timer(TimeSpan.FromSeconds(2)).ObserveOnMainThread().Subscribe(l => Spawn());
					}
					else
					{
						GameOver();
					}
				});
		}

		private void GameOver()
		{
		}
	}
}