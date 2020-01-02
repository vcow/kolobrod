using GameScene.Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace GameScene
{
	[DisallowMultipleComponent, RequireComponent(typeof(Image))]
	public class NavigationButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
#pragma warning disable 649
		[SerializeField] private Vector2 _direction;
		[SerializeField] private Sprite _regularSkin;
		[SerializeField] private Sprite _pressedSkin;

		[Inject] private readonly SignalBus _signalBus;
#pragma warning restore 649

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_pressedSkin) GetComponent<Image>().sprite = _pressedSkin;
			_signalBus.Fire(new MoveSignal(_direction));
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (_regularSkin) GetComponent<Image>().sprite = _regularSkin;
			_signalBus.Fire(new MoveSignal(Vector2.zero));
		}
	}
}