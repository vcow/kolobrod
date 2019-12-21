using AI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameScene
{
	[DisallowMultipleComponent]
	public class GameUiController : MonoBehaviour
	{
		private readonly CompositeDisposable _handlers = new CompositeDisposable();

#pragma warning disable 649
		[SerializeField] private RectTransform _livesContainer;
		[SerializeField] private GameObject _lifePrefab;
		[SerializeField] private Image _progressBar;

		[Inject] private readonly CharacterSpawnZone _characterSpawnZone;
#pragma warning restore 649

		private void Start()
		{
			for (var i = 0; i < _characterSpawnZone.NumLives; ++i)
			{
				Instantiate(_lifePrefab, _livesContainer);
			}

			_handlers.Add(_characterSpawnZone.LivesLeft.Subscribe(OnLivesChanged));
			_handlers.Add(_characterSpawnZone.Health.Subscribe(OnHealthChanged));
		}

		private void OnHealthChanged(float health)
		{
			_progressBar.fillAmount = health;
		}

		private void OnLivesChanged(int livesLeft)
		{
			var livesLose = Mathf.Max(_characterSpawnZone.NumLives - livesLeft, 0);
			foreach (Transform child in _livesContainer)
			{
				var img = child.GetComponent<Image>();
				if (img) img.color = livesLose > 0 ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white;
				--livesLose;
			}
		}

		private void OnDestroy()
		{
			_handlers.Dispose();
		}
	}
}