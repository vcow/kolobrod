using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace Common
{
	[DisallowMultipleComponent, RequireComponent(typeof(AudioListener))]
	public class AudioListenerController : MonoInstaller<AudioListenerController>
	{
		private Transform _transform;
		private Transform _pursued;
		private IDisposable _pursuedDestroyHandler;

		public override void InstallBindings()
		{
			Container.Bind<AudioListenerController>().FromInstance(this).AsSingle();
		}

		private void Awake()
		{
			_transform = transform;
		}

		private void Update()
		{
			if (_pursued) _transform.position = _pursued.position;
		}

		public Transform Pursued
		{
			set
			{
				if (value == _pursued) return;

				_pursued = value;
				_pursuedDestroyHandler?.Dispose();

				if (_pursued != null || !_transform)
				{
					_pursuedDestroyHandler = _pursued.OnDestroyAsObservable().Subscribe(unit =>
					{
						_pursuedDestroyHandler?.Dispose();
						_pursuedDestroyHandler = null;
						_pursued = null;
						_transform.position = Vector3.zero;
					});
				}
				else
				{
					_pursuedDestroyHandler = null;
					transform.position = Vector3.zero;
				}
			}
		}
	}
}