using System.Collections;
using Base.ScreenLocker;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace LoadGameScene
{
	public class LoadGameSceneInstaller : MonoInstaller<LoadGameSceneInstaller>
	{
#pragma warning disable 649
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly ZenjectSceneLoader _sceneLoader;
#pragma warning restore 649

		public override void InstallBindings()
		{
		}

		public override void Start()
		{
			_screenLockerManager.Lock(LockerType.GameLoader, InitGame);
		}

		private void InitGame()
		{
			StartCoroutine(StartGameRoutine());
		}

		private IEnumerator StartGameRoutine()
		{
			yield return new WaitForSeconds(1);
			_sceneLoader.LoadSceneAsync("StartGameScene", LoadSceneMode.Single);
		}
	}
}