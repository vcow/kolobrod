using Base.Activatable;
using Base.ScreenLocker;
using DG.Tweening;
using Helpers.TouchHelper;
using UnityEngine;
using UnityEngine.Assertions;

namespace ScreenLocker
{
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class CommonScreenLockerBase : ScreenLocker<CommonScreenLockerBase>
	{
		private const float FadeDuration = 1f;

		private bool _isStarted;
		private CanvasGroup _canvasGroup;
		private Tween _tween;
		private int _lockerId;

		private void Awake()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
			_canvasGroup.interactable = false;
			_canvasGroup.alpha = 0;
		}

		private void Start()
		{
			_isStarted = true;
			ValidateState();
		}

		protected override void OnDestroy()
		{
			if (_lockerId != 0)
			{
				TouchHelper.Unlock(_lockerId);
			}

			_tween?.Kill();
			base.OnDestroy();
		}

		public override void Activate(bool immediately = false)
		{
			Assert.IsFalse(this.IsActiveOrActivated());
			ActivatableState = immediately ? ActivatableState.Active : ActivatableState.ToActive;
			ValidateState();
		}

		public override void Deactivate(bool immediately = false)
		{
			Assert.IsFalse(this.IsInactiveOrDeactivated());
			ActivatableState = immediately ? ActivatableState.Inactive : ActivatableState.ToInactive;
			ValidateState();
		}

		private void ValidateState()
		{
			if (!_isStarted) return;

			_tween?.Kill();
			_tween = null;

			float fadeDuration;
			switch (ActivatableState)
			{
				case ActivatableState.Active:
					Assert.IsTrue(_lockerId == 0);
					_lockerId = TouchHelper.Lock();
					_canvasGroup.interactable = true;
					_canvasGroup.alpha = 1;
					break;
				case ActivatableState.Inactive:
					if (_lockerId != 0)
					{
						TouchHelper.Unlock(_lockerId);
						_lockerId = 0;
					}

					_canvasGroup.interactable = false;
					_canvasGroup.alpha = 0;
					break;
				case ActivatableState.ToActive:
					Assert.IsTrue(_lockerId == 0);
					_lockerId = TouchHelper.Lock();
					_canvasGroup.interactable = true;

					fadeDuration = FadeDuration * (1f - _canvasGroup.alpha);
					if (fadeDuration < 0.1f)
					{
						ActivatableState = ActivatableState.Active;
					}
					else
					{
						_tween = _canvasGroup.DOFade(1, fadeDuration).SetDelay(0.1f).OnComplete(() =>
						{
							_tween = null;
							ActivatableState = ActivatableState.Active;
						});
					}

					break;
				case ActivatableState.ToInactive:
					fadeDuration = FadeDuration * _canvasGroup.alpha;
					if (fadeDuration < 0.1f)
					{
						if (_lockerId != 0)
						{
							TouchHelper.Unlock(_lockerId);
							_lockerId = 0;
						}

						_canvasGroup.interactable = false;
						ActivatableState = ActivatableState.Inactive;
					}
					else
					{
						_tween = _canvasGroup.DOFade(0, 1).SetDelay(0.1f).OnComplete(() =>
						{
							_tween = null;
							Assert.IsTrue(_lockerId != 0);
							TouchHelper.Unlock(_lockerId);
							_lockerId = 0;
							_canvasGroup.interactable = false;
							ActivatableState = ActivatableState.Inactive;
						});
					}

					break;
			}
		}
	}
}